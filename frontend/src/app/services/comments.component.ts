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

  // reply control
  replyToParentId: string | null = null;
  replyToUserName: string | null = null;

  // сортировка
  sortField: 'userName' | 'email' | 'createdAt' = 'createdAt';
  sortOrder: 'asc' | 'desc' = 'desc';

  constructor(
    private service: CommentsService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.load();
  }

 load() {
  this.service.getComments(this.sortField, this.sortOrder).subscribe({
    next: (data: any) => {
      this.commentsFlat = Array.isArray(data) ? data : [];
      this.commentsTree = this.buildTree(this.commentsFlat);
      this.sortTree(this.commentsTree); // сортировка на фронте
      try { this.cdr.detectChanges(); } catch (e) {}
    },
    error: (err) => console.error('Error loading comments', err)
  });
}



  // создаём дерево из плоского массива
  buildTree(comments: any[]): any[] {
    const map = new Map<string, any>();
    const roots: any[] = [];

    comments.forEach(c => {
      const obj = { ...c, children: [] };
      obj.id = (obj.id ?? obj.Id ?? '').toString();
      obj.parentId = obj.parentId ?? obj.ParentId ?? null;
      if (obj.parentId) obj.parentId = obj.parentId.toString();
      map.set(obj.id, obj);
    });

    map.forEach(c => {
      if (c.parentId) {
        const parent = map.get(c.parentId);
        if (parent) parent.children.push(c);
        else roots.push(c);
      } else roots.push(c);
    });

    return roots;
  }

  // рекурсивная сортировка дерева по выбранному полю
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
    nodes.forEach(n => n.children?.length && this.sortTree(n.children));
  }

  // смена поля сортировки и порядка
  setSort(field: 'userName' | 'email' | 'createdAt') {
    if (this.sortField === field) {
      this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortField = field;
      this.sortOrder = 'asc';
    }
    this.sortTree(this.commentsTree);
  }

  // отправка комментария
  send(parentId?: string | null) {
    const targetParent = parentId ?? this.replyToParentId ?? null;
    if (!this.text?.trim() || !this.userName?.trim() || !this.email?.trim()) return;

    const payload: any = {
      userName: this.userName,
      email: this.email,
      homePage: this.homePage,
      commentText: this.text,
      parentId: targetParent
    };

    this.service.createComment(payload).subscribe({
      next: () => {
        this.text = '';
        this.replyToParentId = null;
        this.replyToUserName = null;
        this.load();
      },
      error: () => {
        alert('Error creating comment');
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
}