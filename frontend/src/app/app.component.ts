import { Component } from '@angular/core';
import { CommentsComponent } from './services/comments.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommentsComponent],
  template: `<h1>Comments</h1><app-comments></app-comments>`
})
export class AppComponent {
  title = 'comments-spa';
}