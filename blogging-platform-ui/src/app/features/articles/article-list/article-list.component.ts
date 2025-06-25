import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ArticleService } from '../../../core/services/article.service';
import { CategoryService } from '../../../core/services/category.service';
import { Article, ArticlePagination } from '../../../core/models/article.model';
import { Category } from '../../../core/models/category.model';
import { PageEvent } from '@angular/material/paginator';
import { finalize } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-article-list',
  templateUrl: './article-list.component.html',
  styleUrls: ['./article-list.component.scss']
})
export class ArticleListComponent implements OnInit {
  articles: Article[] = [];
  categories: Category[] = [];
  selectedCategoryId: number | null = null;
  isLoading = true;
  isLoadingCategories = true;
  error = false;
  
  // Pagination
  totalArticles = 0;
  pageSize = 10;
  currentPage = 1;
  pageSizeOptions: number[] = [5, 10, 25, 50];

  constructor(
    private articleService: ArticleService,
    private categoryService: CategoryService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.currentPage = params['page'] ? parseInt(params['page']) : 1;
      this.pageSize = params['size'] ? parseInt(params['size']) : 10;
      this.selectedCategoryId = params['category'] ? parseInt(params['category']) : null;
      
      this.loadArticles();
    });
    
    this.loadCategories();
  }

  loadArticles(): void {
    this.isLoading = true;
    this.error = false;
    
    const loadArticles = this.selectedCategoryId
      ? this.articleService.getArticlesByCategory(this.selectedCategoryId, this.currentPage, this.pageSize)
      : this.articleService.getArticles(this.currentPage, this.pageSize);
    
    loadArticles
      .pipe(finalize(() => this.isLoading = false))
      .subscribe(
        (response: ArticlePagination) => {
          this.articles = response.articles;
          this.totalArticles = response.totalCount;
        },
        (error) => {
          console.error('Error loading articles', error);
          this.error = true;
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
        }
      );
  }

  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    
    this.updateQueryParams();
  }

  onCategoryChange(categoryId: number | null): void {
    this.selectedCategoryId = categoryId;
    this.currentPage = 1; // Reset to first page when changing category
    
    this.updateQueryParams();
  }

  private updateQueryParams(): void {
    const queryParams: any = {
      page: this.currentPage,
      size: this.pageSize
    };
    
    if (this.selectedCategoryId) {
      queryParams.category = this.selectedCategoryId;
    }
    
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams,
      queryParamsHandling: 'merge'
    });
  }

  getArticleExcerpt(content: string, maxLength: number = 200): string {
    if (!content) return '';
    
    // Remove HTML tags if present
    const plainText = content.replace(/<[^>]*>/g, '');
    
    if (plainText.length <= maxLength) return plainText;
    
    // Find the last space before maxLength to avoid cutting words
    const lastSpace = plainText.substring(0, maxLength).lastIndexOf(' ');
    return plainText.substring(0, lastSpace > 0 ? lastSpace : maxLength) + '...';
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