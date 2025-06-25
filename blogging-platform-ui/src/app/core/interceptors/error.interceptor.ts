import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  constructor(
    private toastr: ToastrService,
    private router: Router,
    private authService: AuthService
  ) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        let errorMessage = 'An unexpected error occurred';
        
        if (error.error instanceof ErrorEvent) {
          // Client-side error
          errorMessage = `Error: ${error.error.message}`;
        } else {
          // Server-side error
          switch (error.status) {
            case 400:
              if (error.error && error.error.message) {
                errorMessage = error.error.message;
              } else {
                errorMessage = 'Bad request';
              }
              break;
            case 401:
              errorMessage = 'Unauthorized. Please log in again.';
              this.authService.logout();
              this.router.navigate(['/auth/login']);
              break;
            case 403:
              errorMessage = 'You do not have permission to perform this action';
              break;
            case 404:
              errorMessage = 'The requested resource was not found';
              break;
            case 405:
              errorMessage = 'The requested operation is not allowed';
              console.error('Method not allowed error:', error);
              break;
            case 500:
              errorMessage = 'Server error. Please try again later';
              break;
            default:
              if (error.error && error.error.message) {
                errorMessage = error.error.message;
              }
          }
        }
        
        // Don't show toastr for 401 errors as we're redirecting to login
        if (error.status !== 401) {
          this.toastr.error(errorMessage);
        }
        
        return throwError(error);
      })
    );
  }
}