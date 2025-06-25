import { Component, OnInit } from '@angular/core';
import { CategoryService } from '../../../core/services/category.service';
import { CategoryWithCount } from '../../../core/models/category.model';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-category-list',
  templateUrl: './category-list.component.html',
  styleUrls: ['./category-list.component.scss']
})
export class CategoryListComponent implements OnInit {
  categories: CategoryWithCount[] = [];
  isLoading = true;
  error = false;

  constructor(private categoryService: CategoryService) { }

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.isLoading = true;
    this.error = false;
    
    this.categoryService.getCategoriesWithCount()
      .pipe(finalize(() => this.isLoading = false))
      .subscribe(
        (categories) => {
          this.categories = categories.sort((a, b) => {
            // Sort by article count (descending)
            return (b.articleCount || 0) - (a.articleCount || 0);
          });
        },
        (error) => {
          console.error('Error loading categories', error);
          this.error = true;
        }
      );
  }
}