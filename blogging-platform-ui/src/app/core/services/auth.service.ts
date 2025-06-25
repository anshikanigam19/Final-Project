import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import {
  User,
  AuthResponse,
  RegisterRequest,
  RegisterWithOtpRequest,
  LoginRequest,
  PasswordResetRequest,
  PasswordResetConfirmation,
  SendOtpRequest,
  VerifyOtpRequest,
  OtpResponse
} from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;
  private tokenExpirationTimer: any;
  private _currentUser = new BehaviorSubject<User | null>(null);

  constructor(
    private http: HttpClient,
    private router: Router
  ) {}

  get currentUser(): Observable<User | null> {
    return this._currentUser.asObservable();
  }

  sendOtp(email: string): Observable<AuthResponse> {
    const sendOtpRequest: SendOtpRequest = { email };
    return this.http.post<AuthResponse>(`${this.apiUrl}/send-otp`, sendOtpRequest)
      .pipe(catchError(this.handleError));
  }

  verifyOtp(email: string, otp: string): Observable<OtpResponse> {
    const verifyOtpRequest: VerifyOtpRequest = { email, otp };
    return this.http.post<OtpResponse>(`${this.apiUrl}/verify-otp`, verifyOtpRequest)
      .pipe(catchError(this.handleError));
  }

  register(registerData: RegisterWithOtpRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, registerData)
      .pipe(
        tap(response => {
          if (response.success && response.token && response.user) {
            this.handleAuthentication(response.token, response.user);
          }
        }),
        catchError(this.handleError)
      );
  }

  login(loginData: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, loginData)
      .pipe(
        tap(response => {
          if (response.success && response.token && response.user) {
            this.handleAuthentication(response.token, response.user);
          }
        }),
        catchError(this.handleError)
      );
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('userData');
    this._currentUser.next(null);
    if (this.tokenExpirationTimer) {
      clearTimeout(this.tokenExpirationTimer);
    }
    this.tokenExpirationTimer = null;
    this.router.navigate(['/']);
  }

  autoLogin(): void {
    const userData = localStorage.getItem('userData');
    if (!userData) {
      return;
    }

    const parsedUser: User = JSON.parse(userData);
    const token = localStorage.getItem('token');

    if (token) {
      this._currentUser.next(parsedUser);
    }
  }

  requestPasswordReset(email: string): Observable<AuthResponse> {
    const resetRequest: PasswordResetRequest = { email };
    return this.http.post<AuthResponse>(`${this.apiUrl}/forgot-password`, resetRequest)
      .pipe(catchError(this.handleError));
  }

  resetPassword(resetData: PasswordResetConfirmation): Observable<AuthResponse> {
    // Transform the request data to match the backend's expected field names with correct casing
    const transformedData = {
      Email: resetData.email,
      Token: resetData.token,
      NewPassword: resetData.newPassword,
      ConfirmNewPassword: resetData.confirmNewPassword
    };
    
    console.log('Sending password reset request with data:', {
      email: transformedData.Email,
      tokenLength: transformedData.Token.length,
      tokenPreview: transformedData.Token.substring(0, 10) + '...',
      newPasswordLength: transformedData.NewPassword.length,
      confirmNewPasswordLength: transformedData.ConfirmNewPassword.length
    });
    
    return this.http.post<AuthResponse>(`${this.apiUrl}/reset-password`, transformedData)
      .pipe(
        tap(response => {
          console.log('Password reset response:', response);
        }),
        catchError(error => {
          console.error('Password reset error:', error);
          return this.handleError(error);
        })
      );
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  private handleAuthentication(token: string, user: User): void {
    localStorage.setItem('token', token);
    localStorage.setItem('userData', JSON.stringify(user));
    this._currentUser.next(user);
  }

  updateUserInfo(user: User): void {
    localStorage.setItem('userData', JSON.stringify(user));
    this._currentUser.next(user);
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