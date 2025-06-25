import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Comment, CommentCreate } from '../models/comment.model';

interface CommentUpdate {
  content: string;
}

@Injectable({
  providedIn: 'root'
})
export class CommentService {
  private apiUrl = `${environment.apiUrl}/comments`;

  constructor(private http: HttpClient) {}

  getCommentsByArticleId(articleId: number): Observable<Comment[]> {
    return this.http.get<Comment[]>(`${this.apiUrl}/article/${articleId}`)
      .pipe(catchError(this.handleError));
  }

  createComment(comment: CommentCreate): Observable<Comment> {
    return this.http.post<Comment>(this.apiUrl, comment)
      .pipe(catchError(this.handleError));
  }

  deleteComment(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`)
      .pipe(catchError(this.handleError));
  }

  updateComment(id: number, content: string): Observable<Comment> {
    return this.http.put<Comment>(`${this.apiUrl}/${id}`, { content })
      .pipe(catchError(this.handleError));
  }

  getUserComments(): Observable<Comment[]> {
    return this.http.get<Comment[]>(`${this.apiUrl}/user`)
      .pipe(catchError(this.handleError));
  }

  private handleError(error: any): Observable<never> {
    let errorMessage = 'An unknown error occurred!';
    if (error.error && error.error.message) {
      errorMessage = error.error.message;
    } else if (error.message) {
      errorMessage = error.message;
    }
    return throwError(errorMessage);
  }
}