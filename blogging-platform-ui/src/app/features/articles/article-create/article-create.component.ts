import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ArticleService } from '../../../core/services/article.service';
import { CategoryService } from '../../../core/services/category.service';
import { ArticleCreate } from '../../../core/models/article.model';
import { Category } from '../../../core/models/category.model';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-article-create',
  templateUrl: './article-create.component.html',
  styleUrls: ['./article-create.component.scss']
})
export class ArticleCreateComponent implements OnInit {
  articleForm: FormGroup;
  categories: Category[] = [];
  isLoading = false;
  isLoadingCategories = true;
  tinymceApiKey = environment.tinymceApiKey;
  selectedImageFile: File | null = null;
  imagePreview: string | null = null;
  imageError: string | null = null;
  maxImageSize = 5 * 1024 * 1024; // 5MB
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

  onSubmit(): void {
    if (this.articleForm.invalid) {
      // Mark all fields as touched to trigger validation messages
      Object.keys(this.articleForm.controls).forEach(key => {
        const control = this.articleForm.get(key);
        control?.markAsTouched();
      });
      return;
    }

    this.isLoading = true;
    const articleData: ArticleCreate = {
      title: this.articleForm.get('title')?.value,
      content: this.articleForm.get('content')?.value,
      categoryIds: this.articleForm.get('categoryIds')?.value
    };

    this.articleService.createArticle(articleData, this.selectedImageFile || undefined)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe(
        (newArticle) => {
          this.toastr.success('Article created successfully');
          this.router.navigate(['/articles', newArticle.id]);
        },
        (error) => {
          console.error('Error creating article', error);
          this.toastr.error('Failed to create article. Please try again.');
        }
      );
  }

  onCancel(): void {
    this.router.navigate(['/articles']);
  }
}