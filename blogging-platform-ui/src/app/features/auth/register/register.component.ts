import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../../../core/services/auth.service';
import { RegisterWithOtpRequest } from '../../../core/models/user.model';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
  registerForm: FormGroup;
  isLoading = false;
  otpSending = false;
  otpVerified = false;
  hidePassword = true;
  hideConfirmPassword = true;

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) {
    this.registerForm = this.formBuilder.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      otp: ['', [
        Validators.required,
        Validators.minLength(6),
        Validators.maxLength(6),
        Validators.pattern(/^[0-9]{6}$/)
      ]],
      password: ['', [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$/)
      ]],
      confirmPassword: ['', [Validators.required]]
    }, { validator: this.passwordMatchValidator });
  }

  passwordMatchValidator(form: FormGroup) {
    const password = form.get('password')?.value;
    const confirmPassword = form.get('confirmPassword')?.value;

    if (password !== confirmPassword) {
      form.get('confirmPassword')?.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    } else {
      form.get('confirmPassword')?.setErrors(null);
      return null;
    }
  }

  sendOtp(): void {
    if (!this.registerForm.get('email')?.valid) {
      return;
    }

    this.otpSending = true;
    const email = this.registerForm.get('email')?.value;

    this.authService.sendOtp(email).subscribe(
      response => {
        this.otpSending = false;
        if (response.success) {
          this.toastr.success('OTP sent to your email', 'Success');
        } else {
          this.toastr.error(response.message || 'Failed to send OTP', 'Error');
        }
      },
      error => {
        this.otpSending = false;
        this.toastr.error(error, 'Failed to send OTP');
      }
    );
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      return;
    }

    this.isLoading = true;
    const email = this.registerForm.get('email')?.value;
    const otp = this.registerForm.get('otp')?.value;

    // First verify the OTP
    this.authService.verifyOtp(email, otp).subscribe(
      verifyResponse => {
        if (verifyResponse.success && verifyResponse.isVerified) {
          // OTP is verified, proceed with registration
          const registerData: RegisterWithOtpRequest = this.registerForm.value;

          this.authService.register(registerData).subscribe(
            response => {
              this.isLoading = false;
              if (response.success) {
                this.toastr.success('Registration successful', 'Success');
                this.router.navigate(['/']);
              } else {
                this.toastr.error(response.message, 'Registration Failed');
              }
            },
            error => {
              this.isLoading = false;
              this.toastr.error(error, 'Registration Failed');
            }
          );
        } else {
          this.isLoading = false;
          this.toastr.error(verifyResponse.message || 'Invalid OTP', 'Verification Failed');
        }
      },
      error => {
        this.isLoading = false;
        this.toastr.error(error, 'OTP Verification Failed');
      }
    );
  }
}