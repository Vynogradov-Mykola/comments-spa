import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class CommentsService {

  api = 'http://localhost:5000/api/comments';
  captchaApi = 'http://localhost:5000/api/captcha';

  constructor(private http: HttpClient) {}

  getComments(sortBy: string, sortOrder: 'asc' | 'desc') {
    return this.http.get(`${this.api}?sortBy=${sortBy}&sortOrder=${sortOrder}&skip=0&take=100`);
  }

  createComment(data: any) {
    return this.http.post(this.api, data);
  }

  getCaptcha() {
    return this.http.get(this.captchaApi, {
      observe: 'response',
      responseType: 'blob'
    });
  }

}