import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ArticleService } from '../../../core/services/article.service';
import { CommentService } from '../../../core/services/comment.service';
import { AuthService } from '../../../core/services/auth.service';
import { Article } from '../../../core/models/article.model';
import { Comment, CommentCreate } from '../../../core/models/comment.model';
import { User } from '../../../core/models/user.model';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { finalize } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-article-detail',
  templateUrl: './article-detail.component.html',
  styleUrls: ['./article-detail.component.scss']
})
export class ArticleDetailComponent implements OnInit {
  article: Article | null = null;
  comments: Comment[] = [];
  currentUser: User | null = null;
  isLoading = true;
  isLoadingComments = true;
  error = false;
  commentForm: FormGroup;
  editCommentForm: FormGroup;
  isSubmittingComment = false;
  isSubmittingEditComment = false;
  editingCommentId: number | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private articleService: ArticleService,
    private commentService: CommentService,
    private authService: AuthService,
    private formBuilder: FormBuilder,
    private toastr: ToastrService,
    private dialog: MatDialog
  ) {
    this.commentForm = this.formBuilder.group({
      content: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(1000)]]
    });
    
    this.editCommentForm = this.formBuilder.group({
      content: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(1000)]]
    });
  }

  ngOnInit(): void {
    this.authService.currentUser.subscribe(user => {
      this.currentUser = user;
    });

    this.route.paramMap.subscribe(params => {
      const articleId = params.get('id');
      if (articleId) {
        this.loadArticle(articleId);
        this.loadComments(articleId);
      } else {
        this.router.navigate(['/not-found']);
      }
    });
  }

  loadArticle(articleId: string): void {
    this.isLoading = true;
    this.error = false;

    this.articleService.getArticleById(Number(articleId))
      .pipe(finalize(() => this.isLoading = false))
      .subscribe(
        (article) => {
          this.article = article;
        },
        (error) => {
          console.error('Error loading article', error);
          this.error = true;
          if (error.status === 404) {
            this.router.navigate(['/not-found']);
          }
        }
      );
  }

  loadComments(articleId: string): void {
    this.isLoadingComments = true;

    this.commentService.getCommentsByArticleId(Number(articleId))
      .pipe(finalize(() => this.isLoadingComments = false))
      .subscribe(
        (comments) => {
          this.comments = comments;
        },
        (error) => {
          console.error('Error loading comments', error);
        }
      );
  }

  onSubmitComment(): void {
    if (this.commentForm.invalid || !this.article) {
      return;
    }

    this.isSubmittingComment = true;
    const commentData: CommentCreate = {
      content: this.commentForm.get('content')?.value,
      articleId: this.article.id
    };

    this.commentService.createComment(commentData)
      .pipe(finalize(() => this.isSubmittingComment = false))
      .subscribe(
        (newComment) => {
          this.comments.unshift(newComment);
          this.commentForm.reset();
          this.toastr.success('Comment added successfully');
        },
        (error) => {
          console.error('Error adding comment', error);
          this.toastr.error('Failed to add comment. Please try again.');
        }
      );
  }

  deleteComment(commentId: number): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '350px',
      data: {
        title: 'Delete Comment',
        message: 'Are you sure you want to delete this comment? This action cannot be undone.',
        confirmText: 'Delete',
        cancelText: 'Cancel'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.commentService.deleteComment(commentId)
          .subscribe(
            () => {
              this.comments = this.comments.filter(comment => comment.id !== commentId);
              this.toastr.success('Comment deleted successfully');
            },
            (error) => {
              console.error('Error deleting comment', error);
              this.toastr.error('Failed to delete comment. Please try again.');
            }
          );
      }
    });
  }

  editComment(comment: Comment): void {
    this.editingCommentId = comment.id;
    this.editCommentForm.patchValue({
      content: comment.content
    });
  }

  cancelEditComment(): void {
    this.editingCommentId = null;
    this.editCommentForm.reset();
  }

  onSubmitEditComment(commentId: number): void {
    if (this.editCommentForm.invalid) {
      return;
    }

    this.isSubmittingEditComment = true;
    const content = this.editCommentForm.get('content')?.value;

    this.commentService.updateComment(commentId, content)
      .pipe(finalize(() => this.isSubmittingEditComment = false))
      .subscribe(
        (updatedComment) => {
          const index = this.comments.findIndex(c => c.id === commentId);
          if (index !== -1) {
            this.comments[index] = updatedComment;
          }
          this.editingCommentId = null;
          this.editCommentForm.reset();
          this.toastr.success('Comment updated successfully');
        },
        (error) => {
          console.error('Error updating comment', error);
          this.toastr.error('Failed to update comment. Please try again.');
        }
      );
  }

  editArticle(): void {
    if (this.article) {
      this.router.navigate(['/articles/edit', this.article.id]);
    }
  }

  deleteArticle(): void {
    if (!this.article) return;

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '350px',
      data: {
        title: 'Delete Article',
        message: 'Are you sure you want to delete this article? This action cannot be undone.',
        confirmText: 'Delete',
        cancelText: 'Cancel'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result && this.article) {
        this.articleService.deleteArticle(this.article.id)
          .subscribe(
            () => {
              this.toastr.success('Article deleted successfully');
              this.router.navigate(['/articles']);
            },
            (error) => {
              console.error('Error deleting article', error);
              this.toastr.error('Failed to delete article. Please try again.');
            }
          );
      }
    });
  }

  isArticleAuthor(): boolean {
    if (!this.article || !this.currentUser) return false;
    return this.article.author?.id === this.currentUser.id;
  }

  isCommentAuthor(comment: Comment): boolean {
    if (!this.currentUser || !comment.user) return false;
    return comment.user.id === this.currentUser.id;
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  getImageUrl(imagePath: string): string {
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