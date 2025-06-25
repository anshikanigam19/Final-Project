import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ArticleService } from '../../../core/services/article.service';
import { Article, ArticlePagination } from '../../../core/models/article.model';
import { PageEvent } from '@angular/material/paginator';
import { finalize } from 'rxjs/operators';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-search-results',
  templateUrl: './search-results.component.html',
  styleUrls: ['./search-results.component.scss']
})
export class SearchResultsComponent implements OnInit {
  searchQuery = '';
  articles: Article[] = [];
  isLoading = true;
  error = false;
  
  // Pagination
  totalArticles = 0;
  pageSize = 10;
  currentPage = 1;
  pageSizeOptions: number[] = [5, 10, 25, 50];
  
  // Search validation
  minSearchLength = 2;
  searchValidationError = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private articleService: ArticleService,
    private sanitizer: DomSanitizer
  ) { }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.searchQuery = params['q'] || '';
      this.currentPage = params['page'] ? parseInt(params['page']) : 1;
      this.pageSize = params['size'] ? parseInt(params['size']) : 10;
      
      if (this.searchQuery) {
        if (this.validateSearch()) {
          this.searchArticles();
        }
      } else {
        this.articles = [];
        this.totalArticles = 0;
        this.isLoading = false;
        this.searchValidationError = '';
      }
    });
  }
  
  validateSearch(): boolean {
    this.searchValidationError = '';
    
    if (this.searchQuery.trim().length < this.minSearchLength) {
      this.searchValidationError = `Search term must be at least ${this.minSearchLength} characters long`;
      this.isLoading = false;
      return false;
    }
    
    return true;
  }

  searchArticles(): void {
    this.isLoading = true;
    this.error = false;

    this.articleService.searchArticles(this.searchQuery, this.currentPage, this.pageSize)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe(
        (response) => {
          // Map the response from the backend (SearchResultDTO) to our component properties
          this.articles = response.results || []; // Backend returns 'Results' property
          this.totalArticles = response.totalCount;
        },
        (error) => {
          console.error('Error searching articles', error);
          this.error = true;
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
        q: this.searchQuery,
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
  
  highlightMatchingText(text: string): SafeHtml {
    if (!text || !this.searchQuery) return text;
    
    try {
      // Create a case-insensitive regular expression from the search query
      const searchRegex = new RegExp(`(${this.escapeRegExp(this.searchQuery)})`, 'gi');
      
      // Replace matches with highlighted spans
      const highlightedText = text.replace(searchRegex, '<span class="highlight-match">$1</span>');
      
      // Sanitize the HTML to prevent XSS attacks
      return this.sanitizer.bypassSecurityTrustHtml(highlightedText);
    } catch (e) {
      // If there's an error (like invalid regex), return the original text
      console.error('Error highlighting text:', e);
      return text;
    }
  }
  
  private escapeRegExp(string: string): string {
    // Escape special characters that could break the regex
    return string.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
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