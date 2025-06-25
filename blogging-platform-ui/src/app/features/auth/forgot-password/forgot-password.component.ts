import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.scss']
})
export class ForgotPasswordComponent {
  forgotPasswordForm: FormGroup;
  isLoading = false;
  requestSent = false;

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) {
    this.forgotPasswordForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  onSubmit(): void {
    if (this.forgotPasswordForm.invalid) {
      return;
    }

    this.isLoading = true;
    const email = this.forgotPasswordForm.get('email')?.value;

    this.authService.requestPasswordReset(email).subscribe(
      response => {
        this.isLoading = false;
        if (response.success) {
          this.requestSent = true;
          this.toastr.success('Password reset instructions have been sent to your email', 'Success');
        } else {
          this.toastr.error(response.message, 'Request Failed');
        }
      },
      error => {
        this.isLoading = false;
        this.toastr.error(error, 'Request Failed');
      }
    );
  }
}