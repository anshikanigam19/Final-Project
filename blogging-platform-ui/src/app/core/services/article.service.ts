import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  Article,
  ArticleCreate,
  ArticleUpdate,
  ArticlePagination,
  SearchResult
} from '../models/article.model';

@Injectable({
  providedIn: 'root'
})
export class ArticleService {
  private apiUrl = `${environment.apiUrl}/articles`;

  constructor(private http: HttpClient) {}

  getArticles(pageIndex: number = 1, pageSize: number = 10): Observable<ArticlePagination> {
    const params = new HttpParams()
      .set('page', pageIndex.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<Article[]>(this.apiUrl, { params })
      .pipe(
        map(articles => {
          return {
            articles: articles,
            totalCount: articles.length,
            pageIndex: pageIndex,
            pageSize: pageSize
          } as ArticlePagination;
        }),
        catchError(this.handleError)
      );
  }

  getArticleById(id: number): Observable<Article> {
    return this.http.get<Article>(`${this.apiUrl}/${id}`)
      .pipe(catchError(this.handleError));
  }

  getArticlesByCategory(categoryId: number, pageIndex: number = 1, pageSize: number = 10): Observable<ArticlePagination> {
    const params = new HttpParams()
      .set('page', pageIndex.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<Article[]>(`${this.apiUrl}/category/${categoryId}`, { params })
      .pipe(
        map(articles => {
          return {
            articles: articles,
            totalCount: articles.length,
            pageIndex: pageIndex,
            pageSize: pageSize
          } as ArticlePagination;
        }),
        catchError(this.handleError)
      );
  }

  getArticlesByAuthor(authorId: number, pageIndex: number = 1, pageSize: number = 10): Observable<ArticlePagination> {
    const params = new HttpParams()
      .set('page', pageIndex.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<Article[]>(`${environment.apiUrl}/users/${authorId}/articles`, { params })
      .pipe(
        map(articles => {
          return {
            articles: articles,
            totalCount: articles.length,
            pageIndex: pageIndex,
            pageSize: pageSize
          } as ArticlePagination;
        }),
        catchError(this.handleError)
      );
  }

  createArticle(article: ArticleCreate, imageFile?: File): Observable<Article> {
    const formData = new FormData();
    formData.append('title', article.title);
    formData.append('content', article.content);
    
    // Add each category ID to the form data
    article.categoryIds.forEach((categoryId) => {
      formData.append('categoryIds', categoryId.toString());
    });
    
    // Add image file if provided
    if (imageFile) {
      formData.append('image', imageFile);
    }
    
    // Log the request details for debugging
    console.log(`Sending POST request to ${this.apiUrl}`);
    // console.log('FormData contents:', Array.from(formData.entries()));
    
    return this.http.post<Article>(this.apiUrl, formData)
      .pipe(catchError(this.handleError));
  }

  updateArticle(id: number, article: ArticleUpdate, imageFile?: File): Observable<Article> {
    const formData = new FormData();
    formData.append('title', article.title);
    formData.append('content', article.content);
    
    // Add each category ID to the form data
    article.categoryIds.forEach((categoryId) => {
      formData.append('categoryIds', categoryId.toString());
    });
    
    // Add flag for removing existing image
    if (article.removeExistingImage) {
      formData.append('removeExistingImage', 'true');
    }
    
    // Add image file if provided
    if (imageFile) {
      formData.append('image', imageFile);
    }
    
    // Log the request details for debugging
    console.log(`Sending PUT request to ${this.apiUrl}/${id}`);
    // Use Array.from to convert FormData entries to array (TypeScript compatible)
    // console.log('FormData contents:', Array.from(formData.entries()));
    
    return this.http.put<Article>(`${this.apiUrl}/${id}`, formData)
      .pipe(
        catchError((error) => {
          console.error('Article update error:', error);
          return this.handleError(error);
        })
      );
  }

  deleteArticle(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`)
      .pipe(catchError(this.handleError));
  }

  searchArticles(query: string, pageIndex: number = 1, pageSize: number = 10): Observable<SearchResult> {
    const params = new HttpParams()
      .set('searchTerm', query) // Changed from 'query' to 'searchTerm' to match backend API
      .set('page', pageIndex.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<SearchResult>(`${this.apiUrl}/search`, { params })
      .pipe(
        catchError(this.handleError)
      );
  }

  getFeaturedArticles(limit: number = 3): Observable<Article[]> {
    const params = new HttpParams().set('limit', limit.toString());
    return this.http.get<Article[]>(`${this.apiUrl}/featured`, { params })
      .pipe(catchError(this.handleError));
  }

  getCurrentUserArticles(pageIndex: number = 1, pageSize: number = 10): Observable<Article[]> {
    const params = new HttpParams()
      .set('page', pageIndex.toString())
      .set('pageSize', pageSize.toString());

    // Get current user's ID from the auth token claims
    const userId = this.getUserIdFromToken();
    if (!userId) {
      return throwError('User not authenticated');
    }

    return this.http.get<Article[]>(`${this.apiUrl}/author/${userId}`, { params })
      .pipe(catchError(this.handleError));
  }

  private getUserIdFromToken(): string | null {
    const token = localStorage.getItem('token');
    if (!token) return null;
    
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.nameid || null;
    } catch {
      return null;
    }
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