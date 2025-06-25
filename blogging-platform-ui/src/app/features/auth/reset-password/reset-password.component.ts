import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../../../core/services/auth.service';
import { PasswordResetConfirmation } from '../../../core/models/user.model';

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.scss']
})
export class ResetPasswordComponent implements OnInit {
  resetPasswordForm: FormGroup;
  isLoading = false;
  hidePassword = true;
  hideConfirmPassword = true;
  token: string = '';
  email: string = '';
  resetComplete = false;

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private toastr: ToastrService
  ) {
    this.resetPasswordForm = this.formBuilder.group({
      newPassword: ['', [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$/)
      ]],
      confirmPassword: ['', [Validators.required]]
    }, { validator: this.passwordMatchValidator });
  }

  ngOnInit(): void {
    // Get token and email from query parameters
    this.route.queryParams.subscribe(params => {
      this.token = params['token'] || '';
      this.email = params['email'] || '';

      if (!this.token || !this.email) {
        this.toastr.error('Invalid password reset link', 'Error');
        this.router.navigate(['/forgot-password']);
      }
    });
  }

  passwordMatchValidator(form: FormGroup) {
    const password = form.get('newPassword')?.value;
    const confirmPassword = form.get('confirmPassword')?.value;

    if (password !== confirmPassword) {
      form.get('confirmPassword')?.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    } else {
      form.get('confirmPassword')?.setErrors(null);
      return null;
    }
  }

  onSubmit(): void {
    if (this.resetPasswordForm.invalid) {
      return;
    }

    this.isLoading = true;
    const resetData: PasswordResetConfirmation = {
      email: this.email,
      token: this.token,
      newPassword: this.resetPasswordForm.get('newPassword')?.value,
      confirmNewPassword: this.resetPasswordForm.get('confirmPassword')?.value
    };
    
    console.log('Reset password data:', resetData);

    this.authService.resetPassword(resetData).subscribe(
      response => {
        this.isLoading = false;
        if (response.success) {
          this.resetComplete = true;
          this.toastr.success('Your password has been reset successfully', 'Success');
        } else {
          this.toastr.error(response.message, 'Password Reset Failed');
        }
      },
      error => {
        this.isLoading = false;
        this.toastr.error(error, 'Password Reset Failed');
      }
    );
  }
}