import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UserService } from '../../core/services/user.service';
import { AuthService } from '../../core/services/auth.service';
import { ArticleService } from '../../core/services/article.service';
import { CommentService } from '../../core/services/comment.service';
import { User } from '../../core/models/user.model';
import { Article } from '../../core/models/article.model';
import { Comment } from '../../core/models/comment.model';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { MatDialog } from '@angular/material/dialog';
import { ChangePasswordComponent } from './change-password/change-password.component';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {
  user: User | null = null;
  userArticles: Article[] = [];
  userComments: Comment[] = [];
  profileForm: FormGroup;
  isLoadingProfile = true;
  isLoadingArticles = true;
  isLoadingComments = true;
  isUpdating = false;
  profileError = false;
  articlesError = false;
  commentsError = false;
  activeTab = 'profile'; // 'profile', 'articles', or 'comments'

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private authService: AuthService,
    private articleService: ArticleService,
    private commentService: CommentService,
    private toastr: ToastrService,
    private dialog: MatDialog
  ) {
    this.profileForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(50)]],
      lastName: ['', [Validators.required, Validators.maxLength(50)]],
      email: [{ value: '', disabled: true }],
      bio: ['', Validators.maxLength(500)]
    });
  }

  ngOnInit(): void {
    this.loadUserProfile();
    this.loadUserArticles();
    this.loadUserComments();
  }

  loadUserProfile(): void {
    this.isLoadingProfile = true;
    this.profileError = false;

    this.userService.getCurrentUser()
      .pipe(finalize(() => this.isLoadingProfile = false))
      .subscribe(
        (user) => {
          this.user = user;
          this.profileForm.patchValue({
            firstName: user.firstName,
            lastName: user.lastName,
            email: user.email,
            bio: user.bio || ''
          });
        },
        (error) => {
          console.error('Error loading user profile', error);
          this.profileError = true;
          this.toastr.error('Failed to load profile information');
        }
      );
  }

  loadUserArticles(): void {
    this.isLoadingArticles = true;
    this.articlesError = false;

    this.articleService.getCurrentUserArticles()
      .pipe(finalize(() => this.isLoadingArticles = false))
      .subscribe(
        (articles) => {
          this.userArticles = articles;
        },
        (error) => {
          console.error('Error loading user articles', error);
          this.articlesError = true;
        }
      );
  }

  loadUserComments(): void {
    if (!this.authService.getToken()) {
      this.commentsError = true;
      this.toastr.error('Please log in to view your comments');
      return;
    }

    this.isLoadingComments = true;
    this.commentsError = false;

    this.commentService.getUserComments()
      .pipe(finalize(() => this.isLoadingComments = false))
      .subscribe(
        (comments) => {
          this.userComments = comments;
        },
        (error) => {
          console.error('Error loading user comments:', error);
          this.commentsError = true;
          if (error.status === 405) {
            this.toastr.error('Unable to load comments. The server does not support this operation.');
          } else {
            this.toastr.error('Failed to load comments. Please try again later.');
          }
        }
      );
  }

  updateProfile(): void {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }

    this.isUpdating = true;
    const updatedUser = {
      ...this.profileForm.value,
      email: this.user?.email // Keep the email from the current user object
    };

    this.userService.updateProfile(updatedUser)
      .pipe(finalize(() => this.isUpdating = false))
      .subscribe(
        (user) => {
          this.user = user;
          this.toastr.success('Profile updated successfully');
          // Update user info in auth service if needed
          this.authService.updateUserInfo(user);
        },
        (error) => {
          console.error('Error updating profile', error);
          this.toastr.error('Failed to update profile');
        }
      );
  }

  changePassword(): void {
    const dialogRef = this.dialog.open(ChangePasswordComponent, {
      width: '400px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === true) {
        this.toastr.success('Password changed successfully');
      }
    });
  }
  
  setActiveTab(tab: string): void {
    this.activeTab = tab;
  }

  getArticleExcerpt(content: string, maxLength: number = 150): string {
    if (!content) return '';
    
    // Remove HTML tags if present
    const plainText = content.replace(/<[^>]*>/g, '');
    
    if (plainText.length <= maxLength) return plainText;
    
    // Find the last space before maxLength to avoid cutting words
    const lastSpace = plainText.substring(0, maxLength).lastIndexOf(' ');
    return plainText.substring(0, lastSpace > 0 ? lastSpace : maxLength) + '...';
  }

  getImageUrl(imagePath: string): string {
    if (!imagePath) return '';
    
    // Check if the path already starts with http:// or https://
    if (imagePath.startsWith('http://') || imagePath.startsWith('https://')) {
      return imagePath;
    }
    
    // If the path starts with a slash, remove it to avoid double slashes
    if (imagePath.startsWith('/')) {
      imagePath = imagePath.substring(1);
    }
    
    // Construct the full URL by combining the API base URL with the image path
    // The API URL is typically something like 'http://localhost:5000/api'
    // We need to remove the '/api' part to get the base URL
    const baseUrl = environment.apiUrl.replace('/api', '');
    return `${baseUrl}/${imagePath}`;
  }
}