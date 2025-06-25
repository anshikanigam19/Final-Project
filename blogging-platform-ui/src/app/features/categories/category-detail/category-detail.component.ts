import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CategoryService } from '../../../core/services/category.service';
import { ArticleService } from '../../../core/services/article.service';
import { Category } from '../../../core/models/category.model';
import { Article, ArticlePagination } from '../../../core/models/article.model';
import { PageEvent } from '@angular/material/paginator';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-category-detail',
  templateUrl: './category-detail.component.html',
  styleUrls: ['./category-detail.component.scss']
})
export class CategoryDetailComponent implements OnInit {
  category: Category | null = null;
  articles: Article[] = [];
  isLoadingCategory = true;
  isLoadingArticles = true;
  categoryError = false;
  articlesError = false;
  
  // Pagination
  totalArticles = 0;
  pageSize = 10;
  currentPage = 1;
  pageSizeOptions: number[] = [5, 10, 25, 50];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private categoryService: CategoryService,
    private articleService: ArticleService
  ) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const categoryId = params.get('id');
      if (categoryId) {
        this.loadCategory(categoryId);
        this.loadArticles(categoryId);
      } else {
        this.router.navigate(['/not-found']);
      }
    });
    
    this.route.queryParams.subscribe(params => {
      this.currentPage = params['page'] ? parseInt(params['page']) : 1;
      this.pageSize = params['size'] ? parseInt(params['size']) : 10;
      
      const categoryId = this.route.snapshot.paramMap.get('id');
      if (categoryId) {
        this.loadArticles(categoryId);
      }
    });
  }

  loadCategory(categoryId: string): void {
    this.isLoadingCategory = true;
    this.categoryError = false;

    this.categoryService.getCategoryById(parseInt(categoryId))
      .pipe(finalize(() => this.isLoadingCategory = false))
      .subscribe(
        (category) => {
          this.category = category;
        },
        (error) => {
          console.error('Error loading category', error);
          this.categoryError = true;
          if (error.status === 404) {
            this.router.navigate(['/not-found']);
          }
        }
      );
  }

  loadArticles(categoryId: string): void {
    this.isLoadingArticles = true;
    this.articlesError = false;

    this.articleService.getArticlesByCategory(parseInt(categoryId), this.currentPage, this.pageSize)
      .pipe(finalize(() => this.isLoadingArticles = false))
      .subscribe(
        (response: ArticlePagination) => {
          this.articles = response.articles;
          this.totalArticles = response.totalCount;
        },
        (error) => {
          console.error('Error loading articles', error);
          this.articlesError = true;
        }
      );
  }

  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    
    this.updateQueryParams();
  }

  private updateQueryParams(): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        page: this.currentPage,
        size: this.pageSize
      },
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
}