import { Component, OnInit } from '@angular/core';
import { ArticleService } from '../../core/services/article.service';
import { CategoryService } from '../../core/services/category.service';
import { Article } from '../../core/models/article.model';
import { CategoryWithCount } from '../../core/models/category.model';
import { finalize } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  featuredArticles: Article[] = [];
  recentArticles: Article[] = [];
  popularCategories: CategoryWithCount[] = [];
  isLoadingArticles = true;
  isLoadingCategories = true;
  articlesError = false;
  categoriesError = false;

  constructor(
    private articleService: ArticleService,
    private categoryService: CategoryService
  ) { }

  ngOnInit(): void {
    this.loadFeaturedArticles();
    this.loadRecentArticles();
    this.loadPopularCategories();
  }

  loadFeaturedArticles(): void {
    this.articleService.getFeaturedArticles()
      .pipe(finalize(() => this.isLoadingArticles = false))
      .subscribe(
        (articles) => {
          this.featuredArticles = articles;
        },
        (error) => {
          console.error('Error loading featured articles', error);
          this.articlesError = true;
        }
      );
  }

  loadRecentArticles(): void {
    this.articleService.getArticles(1, 6)
      .subscribe(
        (response) => {
          this.recentArticles = response.articles;
        },
        (error) => {
          console.error('Error loading recent articles', error);
          this.articlesError = true;
        }
      );
  }

  loadPopularCategories(): void {
    this.categoryService.getCategoriesWithCount()
      .pipe(finalize(() => this.isLoadingCategories = false))
      .subscribe(
        (categories) => {
          this.popularCategories = categories
            .sort((a, b) => (b.articleCount || 0) - (a.articleCount || 0))
            .slice(0, 6);
        },
        (error) => {
          console.error('Error loading popular categories', error);
          this.categoriesError = true;
        }
      );
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