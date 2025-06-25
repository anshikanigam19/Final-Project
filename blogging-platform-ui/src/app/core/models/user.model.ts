export interface User {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  createdAt: Date;
  lastLogin?: Date;
  bio?: string;
}

export interface AuthResponse {
  success: boolean;
  message: string;
  token?: string;
  user?: User;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface RegisterWithOtpRequest {
  firstName: string;
  lastName: string;
  email: string;
  otp: string;
  password: string;
  confirmPassword: string;
}

export interface SendOtpRequest {
  email: string;
}

export interface VerifyOtpRequest {
  email: string;
  otp: string;
}

export interface OtpResponse {
  success: boolean;
  message: string;
  isVerified?: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface PasswordResetRequest {
  email: string;
}

export interface PasswordResetConfirmation {
  email: string;
  token: string;
  newPassword: string;
  confirmNewPassword: string;
}