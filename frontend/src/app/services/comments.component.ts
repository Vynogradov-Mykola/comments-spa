import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CommentsService } from './comments.service';

@Component({
  selector: 'app-comments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './comments.component.html',
  styleUrls: ['./comments.component.css']
})
export class CommentsComponent implements OnInit {

  commentsFlat: any[] = [];
  commentsTree: any[] = [];

  // form
  text = '';
  userName = '';
  email = '';
  homePage = '';
page = 1;
pageSize = 25;
totalComments = 0;
totalPages = 0;
  // CAPTCHA
  captchaImage = '';
  captchaId = '';
  captchaCode = '';

  // reply control
  replyToParentId: string | null = null;
  replyToUserName: string | null = null;

  viewerOpen = false;
  viewerType: 'image' | 'text' | null = null;
  viewerContent = '';

  // sorting
  sortField: 'userName' | 'email' | 'createdAt' = 'createdAt';
  sortOrder: 'asc' | 'desc' = 'desc';

  constructor(
    private service: CommentsService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.load();
    this.loadCaptcha();
  }

 load() {
  this.service.getComments(this.sortField, this.sortOrder).subscribe({
    next: (data: any) => {

      this.commentsFlat = Array.isArray(data) ? data : [];

      // 1. СНАЧАЛА сортируем весь список
      this.sortFlat(this.commentsFlat);

      // 2. считаем страницы
      this.totalComments = this.commentsFlat.length;
      this.totalPages = Math.ceil(this.totalComments / this.pageSize);

      // 3. берём нужную страницу
      const start = (this.page - 1) * this.pageSize;
      const end = start + this.pageSize;
      const pageComments = this.commentsFlat.slice(start, end);

      // 4. строим дерево
      this.commentsTree = this.buildTree(pageComments);

      // 5. Подготавливаем HTML для безопасного рендеринга
      this.prepareCommentHtml(this.commentsTree);

      try { this.cdr.detectChanges(); } catch {}

    },
    error: (err) => console.error(err)
  });
}
prepareCommentHtml(comments: any[]) {
  comments.forEach(c => {
    // Берем текст комментария
    let text = c.commentText ?? c.CommentText;

    if (text) {
      // Добавляем target="_blank" и rel="noopener noreferrer" к ссылкам
      text = text.replace(
        /<a /g,
        '<a target="_blank" rel="noopener noreferrer" '
      );

      // Сохраняем в новое поле для рендеринга
      c.safeHtml = text;
    }

    // Рекурсивно для детей
    if (c.children?.length) this.prepareCommentHtml(c.children);
  });
}
sortFlat(arr: any[]) {

  arr.sort((a, b) => {

    let valA = a[this.sortField];
    let valB = b[this.sortField];

    if (this.sortField === 'createdAt') {
      valA = new Date(valA).getTime();
      valB = new Date(valB).getTime();
    }

    if (valA < valB) return this.sortOrder === 'asc' ? -1 : 1;
    if (valA > valB) return this.sortOrder === 'asc' ? 1 : -1;

    return 0;
  });

}
nextPage() {
  if (this.page < this.totalPages) {
    this.page++;
    this.load();
  }
}

prevPage() {
  if (this.page > 1) {
    this.page--;
    this.load();
  }
}
openImage(comment: any) {

  this.viewerType = 'image';
  this.viewerContent = this.getFileDataUrl(comment) ?? '';
  this.viewerOpen = true;

}
openText(comment: any) {

  this.viewerType = 'text';
  this.viewerContent = this.decodeBase64ToText(comment.fileBase64);
  this.viewerOpen = true;

}
closeViewer() {

  this.viewerOpen = false;
  this.viewerContent = '';
  this.viewerType = null;

}
  loadCaptcha() {
  this.service.getCaptcha().subscribe(res => {

    const blob = res.body!;
    const reader = new FileReader();

    reader.onload = () => {
      this.captchaImage = reader.result as string;
    };

    reader.readAsDataURL(blob);

    // ✅ Берём правильный заголовок с X-
    this.captchaId = res.headers.get('X-Captcha-Id') || '';

    console.log('Captcha ID:', this.captchaId);
  });
}
// Дополнительно в CommentsComponent
selectedFile: File | null = null;
previewImage: string | null = null;
previewText: string | null = null;

// Выбор файла
onFileSelected(event: any) {
  const file: File = event.target.files[0];
  if (!file) return;

  const ext = file.name.split('.').pop()?.toLowerCase();

  // --- Изображение ---
  if (['jpg', 'jpeg', 'png', 'gif'].includes(ext!)) {
    const img = new Image();
    const reader = new FileReader();

    reader.onload = e => {
      img.src = e.target?.result as string;
      img.onload = () => {
        const maxWidth = 320;
        const maxHeight = 240;
        let { width, height } = img;

        if (width > maxWidth || height > maxHeight) {
          const ratio = Math.min(maxWidth / width, maxHeight / height);
          width *= ratio;
          height *= ratio;

          const canvas = document.createElement('canvas');
          canvas.width = width;
          canvas.height = height;
          const ctx = canvas.getContext('2d');
          ctx?.drawImage(img, 0, 0, width, height);

          this.previewImage = canvas.toDataURL(ext === 'png' ? 'image/png' : 'image/jpeg');

          canvas.toBlob(blob => {
            this.selectedFile = new File([blob!], file.name, { type: file.type });
          }, file.type);
        } else {
          this.previewImage = reader.result as string;
          this.selectedFile = file;
        }
      };
    };

    reader.readAsDataURL(file);
    this.previewText = null;
  }
  // --- Текстовый файл ---
  else if (ext === 'txt') {
    if (file.size > 100 * 1024) {
      alert('Text file too large (max 100kb)');
      return;
    }

    const reader = new FileReader();
    reader.onload = e => this.previewText = e.target?.result as string;
    reader.readAsText(file);

    this.selectedFile = file;
    this.previewImage = null;
  } else {
    alert('Unsupported file type');
  }
}
  buildTree(comments: any[]): any[] {

    const map = new Map<string, any>();
    const roots: any[] = [];

    comments.forEach(c => {

      const obj = { ...c, children: [] };

      obj.id = (obj.id ?? obj.Id ?? '').toString();
      obj.parentId = obj.parentId ?? obj.ParentId ?? null;

      if (obj.parentId)
        obj.parentId = obj.parentId.toString();

      map.set(obj.id, obj);
    });

    map.forEach(c => {

      if (c.parentId) {

        const parent = map.get(c.parentId);

        if (parent)
          parent.children.push(c);
        else
          roots.push(c);

      } else
        roots.push(c);

    });

    return roots;
  }

  sortTree(nodes: any[]) {

    const compare = (a: any, b: any) => {

      let valA = a[this.sortField] ?? '';
      let valB = b[this.sortField] ?? '';

      if (this.sortField === 'createdAt') {

        valA = new Date(valA).getTime();
        valB = new Date(valB).getTime();

      } else {

        valA = valA.toString().toLowerCase();
        valB = valB.toString().toLowerCase();

      }

      if (valA < valB) return this.sortOrder === 'asc' ? -1 : 1;
      if (valA > valB) return this.sortOrder === 'asc' ? 1 : -1;

      return 0;
    };

    nodes.sort(compare);

    nodes.forEach(n =>
      n.children?.length && this.sortTree(n.children)
    );
  }

  setSort(field: 'userName' | 'email' | 'createdAt') {

  if (this.sortField === field)
    this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
  else {
    this.sortField = field;
    this.sortOrder = 'asc';
  }

  this.page = 1;   // сброс на первую страницу
  this.load();     // ✅ заново загрузить и отсортировать
}
  
// Проверка типа файла
isImage(fileName: string | null | undefined): boolean {
  return !!fileName && /\.(jpg|jpeg|png|gif)$/i.test(fileName);
}

isText(fileName: string | null | undefined): boolean {
  return !!fileName && /\.txt$/i.test(fileName);
}

// Генерация DataURL
getFileDataUrl(comment: any): string | null {
  if (!comment.fileBase64 || !comment.fileContentType) return null;
  return `data:${comment.fileContentType};base64,${comment.fileBase64}`;
}

// Декодирование txt
decodeBase64ToText(base64: string): string {
  try {
    return decodeURIComponent(
      atob(base64)
        .split('')
        .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
  } catch {
    return '';
  }
}
validateForm(): string | null {
  if (!this.userName.trim()) return "User name is required";
  if (!this.email.trim()) return "Email is required";
  if (this.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.email))
    return "Invalid email format";
  if (!this.text?.trim() && !this.previewText && !this.selectedFile)
    return "Comment or file required";
  if (!this.captchaCode.trim()) return "CAPTCHA is required";
  return null;
}
send(parentId?: string | null) {
  const targetParent = parentId ?? this.replyToParentId ?? null;

  if (!this.text?.trim() && !this.previewText && !this.selectedFile)
    return; // нельзя отправить пустой комментарий
const clientError = this.validateForm();
  if (clientError) {
    alert(clientError);
    return;
  }
  const formData = new FormData();
  formData.append('userName', this.userName);
  formData.append('email', this.email);
  formData.append('homePage', this.homePage);
  formData.append('commentText', this.text);
  formData.append('captchaId', this.captchaId);
  formData.append('captchaCode', this.captchaCode);
  if (targetParent) formData.append('parentId', targetParent);
  if (this.selectedFile) formData.append('file', this.selectedFile);
  
  this.service.createComment(formData).subscribe({
    next: () => {
      this.text = '';
      this.selectedFile = null;
      this.previewImage = null;
      this.previewText = null;
      this.replyToParentId = null;
      this.replyToUserName = null;
      this.captchaCode = '';
      this.load();
      this.loadCaptcha();
    },
   error: (err: any) => {
    // Попытка показать сообщение от сервера
    let msg = 'Unknown error';
    
    // err.error обычно содержит строку с сообщением
    if (typeof err.error === 'string') {
      msg = err.error;
    } 
    // Если сервер вернул JSON { message: "..." }
    else if (err.error?.message) {
      msg = err.error.message;
    }

    alert(msg);       // ✅ Показываем реальное сообщение
    this.loadCaptcha(); // обновляем капчу
  }
  });
}

  reply(comment: any) {

    this.replyToParentId = comment.id ?? comment.Id;
    this.replyToUserName = comment.userName ?? comment.UserName ?? null;

    this.text = `@${this.replyToUserName ?? 'reply'} `;
  }

  cancelReply() {

    this.replyToParentId = null;
    this.replyToUserName = null;
    this.text = '';
  }
previewHtml: string | null = null;

previewComment() {
  let text = this.previewText ?? this.text;
  if (!text) {
    alert("Nothing to preview");
    return;
  }

  // Добавляем target="_blank" к ссылкам
  text = text.replace(
    /<a /g,
    '<a target="_blank" rel="noopener noreferrer" '
  );

  this.previewHtml = text;
}
insertTag(tag: string) {
  const textarea = document.querySelector('textarea') as HTMLTextAreaElement;
  if (!textarea) return;

  const start = textarea.selectionStart;
  const end = textarea.selectionEnd;
  const selected = textarea.value.substring(start, end);
  const before = textarea.value.substring(0, start);
  const after = textarea.value.substring(end);

  textarea.value = `${before}<${tag}>${selected}</${tag}>${after}`;
  textarea.focus();
  this.text = textarea.value;
}

insertLink() {
  const url = prompt("Enter URL");
  if (!url) return;

  const textarea = document.querySelector('textarea') as HTMLTextAreaElement;
  if (!textarea) return;

  const start = textarea.selectionStart;
  const end = textarea.selectionEnd;
  const selected = textarea.value.substring(start, end) || "link text";
  const before = textarea.value.substring(0, start);
  const after = textarea.value.substring(end);

  textarea.value = `${before}<a href="${url}">${selected}</a>${after}`;
  textarea.focus();
  this.text = textarea.value;
}
}