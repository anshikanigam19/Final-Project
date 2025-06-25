import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ArticleService } from '../../../core/services/article.service';
import { CategoryService } from '../../../core/services/category.service';
import { Article, ArticleUpdate } from '../../../core/models/article.model';
import { Category } from '../../../core/models/category.model';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-article-edit',
  templateUrl: './article-edit.component.html',
  styleUrls: ['./article-edit.component.scss']
})
export class ArticleEditComponent implements OnInit {
  articleForm: FormGroup;
  article: Article | null = null;
  categories: Category[] = [];
  isLoading = false;
  isLoadingArticle = true;
  isLoadingCategories = true;
  error = false;
  tinymceApiKey = environment.tinymceApiKey;
  selectedImageFile: File | null = null;
  imagePreview: string | null = null;
  imageError: string | null = null;
  maxImageSize = 5 * 1024 * 1024; // 5MB
  removeImageFlag = false;
  editorConfig = {
    height: 400,
    menubar: true,
    plugins: [
      'advlist autolink lists link image charmap print preview anchor',
      'searchreplace visualblocks code fullscreen',
      'insertdatetime media table paste code help wordcount'
    ],
    toolbar:
      'undo redo | formatselect | bold italic backcolor | \
      alignleft aligncenter alignright alignjustify | \
      bullist numlist outdent indent | removeformat | help'
  };

  constructor(
    private formBuilder: FormBuilder,
    private articleService: ArticleService,
    private categoryService: CategoryService,
    private route: ActivatedRoute,
    private router: Router,
    private toastr: ToastrService
  ) {
    this.articleForm = this.formBuilder.group({
      title: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(100)]],
      content: ['', [Validators.required, Validators.minLength(50)]],
      categoryIds: [[], Validators.required]
      // Image is handled separately
    });
  }

  ngOnInit(): void {
    this.loadCategories();
    
    this.route.paramMap.subscribe(params => {
      const articleId = params.get('id');
      if (articleId) {
        this.loadArticle(articleId);
      } else {
        this.router.navigate(['/not-found']);
      }
    });
  }

  loadArticle(articleId: string): void {
    this.isLoadingArticle = true;
    this.error = false;

    this.articleService.getArticleById(parseInt(articleId))
      .pipe(finalize(() => this.isLoadingArticle = false))
      .subscribe(
        (article) => {
          this.article = article;
          this.populateForm(article);
        },
        (error) => {
          console.error('Error loading article', error);
          this.error = true;
          if (error.status === 404) {
            this.toastr.error('Article not found');
            this.router.navigate(['/not-found']);
          } else {
            this.toastr.error('Failed to load article. Please try again.');
          }
        }
      );
  }

  loadCategories(): void {
    this.isLoadingCategories = true;
    
    this.categoryService.getAllCategories()
      .pipe(finalize(() => this.isLoadingCategories = false))
      .subscribe(
        (categories) => {
          this.categories = categories;
        },
        (error) => {
          console.error('Error loading categories', error);
          this.toastr.error('Failed to load categories. Please try again.');
        }
      );
  }

  populateForm(article: Article): void {
    // Extract category IDs from the article's categories
    const categoryIds = article.categories?.map(category => category.id) || [];
    
    this.articleForm.patchValue({
      title: article.title,
      content: article.content,
      categoryIds: categoryIds
    });
    
    // Reset image-related properties
    this.selectedImageFile = null;
    this.imagePreview = null;
    this.removeImageFlag = false;
  }
  
  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      
      // Validate file type
      if (!file.type.startsWith('image/')) {
        this.imageError = 'Please select a valid image file';
        return;
      }
      
      // Validate file size
      if (file.size > this.maxImageSize) {
        this.imageError = 'Image size should not exceed 5MB';
        return;
      }
      
      this.selectedImageFile = file;
      this.imageError = null;
      this.removeImageFlag = false;
      
      // Create preview
      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreview = reader.result as string;
      };
      reader.readAsDataURL(file);
    }
  }
  
  removeSelectedImage(): void {
    this.selectedImageFile = null;
    this.imagePreview = null;
  }
  
  removeExistingImage(): void {
    this.removeImageFlag = true;
  }

  onSubmit(): void {
    if (this.articleForm.invalid || !this.article) {
      // Mark all fields as touched to trigger validation messages
      Object.keys(this.articleForm.controls).forEach(key => {
        const control = this.articleForm.get(key);
        control?.markAsTouched();
      });
      return;
    }

    this.isLoading = true;
    const articleData: ArticleUpdate = {
      title: this.articleForm.get('title')?.value || '',
      content: this.articleForm.get('content')?.value || '',
      categoryIds: this.articleForm.get('categoryIds')?.value || [],
      removeExistingImage: this.removeImageFlag
    };

    // Log the data being sent to help with debugging
    console.log('Updating article with data:', articleData);
    console.log('Article ID:', this.article.id);
    console.log('Image file:', this.selectedImageFile);

    this.articleService.updateArticle(this.article.id, articleData, this.selectedImageFile || undefined)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (updatedArticle) => {
          this.toastr.success('Article updated successfully');
          this.router.navigate(['/articles', updatedArticle.id]);
        },
        error: (error) => {
          console.error('Error updating article', error);
          this.toastr.error('Failed to update article. Please try again.');
        }
      });
    }

  onCancel(): void {
    if (this.article) {
      this.router.navigate(['/articles', this.article.id]);
    } else {
      this.router.navigate(['/articles']);
    }
  }
}