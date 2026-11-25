const http = require('http');
const fs = require('fs');
const path = require('path');
const { randomUUID } = require('crypto');

const PORT = Number(process.env.PORT || 8080);
const ROOT = __dirname;
const PUBLIC_DIR = path.join(ROOT, 'public');
const DATA_FILE = path.join(ROOT, 'data', 'tasks.json');
const MIME = { '.html': 'text/html; charset=utf-8', '.css': 'text/css; charset=utf-8', '.js': 'text/javascript; charset=utf-8', '.json': 'application/json; charset=utf-8', '.svg': 'image/svg+xml' };

function localDate() {
  const now = new Date();
  return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`;
}
function seedTasks() {
  const today = localDate();
  const base = [
    { title: 'Thực hành HTML có sử dụng Animation', description: 'Tạo trang HTML sinh động với keyframes, transition và hiệu ứng chuyển động.', category: 'Học tập', priority: 'high', status: 'done', dueTime: '08:30' },
    { title: 'Cài đặt Node.js', description: 'Cài Node.js và kiểm tra phiên bản bằng terminal.', category: 'Học tập', priority: 'medium', status: 'done', dueTime: '09:30' },
    { title: 'Cài đặt để mở trang localhost:8080', description: 'Khởi chạy web server local và kiểm tra trang trên trình duyệt.', category: 'Học tập', priority: 'high', status: 'in-progress', dueTime: '10:30' },
    { title: 'Thực hành làm Todo App', description: 'Xây dựng ứng dụng quản lý task với giao diện hiện đại và thao tác CRUD.', category: 'Dự án', priority: 'high', status: 'todo', dueTime: '13:30' }
  ];
  return base.map((task, index) => ({ id: randomUUID(), ...task, dueDate: today, createdAt: new Date(Date.now() - (base.length - index) * 3600000).toISOString(), updatedAt: new Date().toISOString() }));
}
function ensureStore() {
  fs.mkdirSync(path.dirname(DATA_FILE), { recursive: true });
  if (!fs.existsSync(DATA_FILE)) fs.writeFileSync(DATA_FILE, JSON.stringify(seedTasks(), null, 2));
}
function readTasks() { ensureStore(); return JSON.parse(fs.readFileSync(DATA_FILE, 'utf8')); }
function saveTasks(tasks) { fs.writeFileSync(DATA_FILE, JSON.stringify(tasks, null, 2)); }
function send(res, status, body, type = 'application/json; charset=utf-8') {
  res.writeHead(status, { 'Content-Type': type, 'Cache-Control': 'no-store' });
  res.end(type.includes('json') ? JSON.stringify(body) : body);
}
function readBody(req) {
  return new Promise((resolve, reject) => {
    let raw = '';
    req.on('data', chunk => { raw += chunk; if (raw.length > 1e6) req.destroy(); });
    req.on('end', () => { try { resolve(raw ? JSON.parse(raw) : {}); } catch { reject(new Error('JSON không hợp lệ')); } });
    req.on('error', reject);
  });
}
function validTask(input) {
  return input && typeof input.title === 'string' && input.title.trim() &&
    ['todo', 'in-progress', 'done'].includes(input.status) &&
    ['low', 'medium', 'high'].includes(input.priority) &&
    typeof input.dueDate === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(input.dueDate);
}

const server = http.createServer(async (req, res) => {
  const url = new URL(req.url, `http://${req.headers.host || 'localhost'}`);
  if (url.pathname.startsWith('/api/')) {
    try {
      if (url.pathname === '/api/health' && req.method === 'GET') return send(res, 200, { ok: true, storage: 'json-file' });
      if (url.pathname === '/api/tasks' && req.method === 'GET') {
        let tasks = readTasks();
        const date = url.searchParams.get('date');
        if (date) tasks = tasks.filter(task => task.dueDate === date);
        return send(res, 200, tasks.sort((a, b) => (a.dueTime || '').localeCompare(b.dueTime || '')));
      }
      if (url.pathname === '/api/tasks' && req.method === 'POST') {
        const body = await readBody(req);
        if (!validTask(body)) return send(res, 400, { error: 'Thông tin task chưa hợp lệ.' });
        const now = new Date().toISOString();
        const task = { id: randomUUID(), title: body.title.trim(), description: String(body.description || '').trim(), category: String(body.category || 'Cá nhân').trim(), priority: body.priority, status: body.status, dueDate: body.dueDate, dueTime: body.dueTime || '', createdAt: now, updatedAt: now };
        const tasks = readTasks(); tasks.push(task); saveTasks(tasks);
        return send(res, 201, task);
      }
      const match = url.pathname.match(/^\/api\/tasks\/([\w-]+)$/);
      if (match) {
        const tasks = readTasks(); const index = tasks.findIndex(task => task.id === match[1]);
        if (index < 0) return send(res, 404, { error: 'Không tìm thấy task.' });
        if (req.method === 'PATCH') {
          const body = await readBody(req); const next = { ...tasks[index], ...body, id: tasks[index].id, updatedAt: new Date().toISOString() };
          if (!validTask(next)) return send(res, 400, { error: 'Thông tin task chưa hợp lệ.' });
          next.title = next.title.trim(); next.description = String(next.description || '').trim(); next.category = String(next.category || 'Cá nhân').trim();
          tasks[index] = next; saveTasks(tasks); return send(res, 200, next);
        }
        if (req.method === 'DELETE') { const [deleted] = tasks.splice(index, 1); saveTasks(tasks); return send(res, 200, deleted); }
      }
      return send(res, 404, { error: 'API không tồn tại.' });
    } catch (error) { return send(res, 400, { error: error.message || 'Không thể xử lý yêu cầu.' }); }
  }

  const requested = url.pathname === '/' ? 'index.html' : decodeURIComponent(url.pathname).replace(/^\/+/, '');
  const file = path.resolve(PUBLIC_DIR, requested);
  if (!file.startsWith(PUBLIC_DIR + path.sep)) return send(res, 403, 'Forbidden', 'text/plain; charset=utf-8');
  fs.readFile(file, (error, contents) => {
    if (error) return send(res, 404, 'Không tìm thấy trang.', 'text/plain; charset=utf-8');
    send(res, 200, contents, MIME[path.extname(file)] || 'application/octet-stream');
  });
});

ensureStore();
server.listen(PORT, '127.0.0.1', () => console.log(`Todo App đang chạy tại http://localhost:${PORT}`));
