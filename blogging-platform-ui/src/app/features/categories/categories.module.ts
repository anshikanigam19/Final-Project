import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';

// Material Imports
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule } from '@angular/material/paginator';

// Components
import { CategoryListComponent } from './category-list/category-list.component';
import { CategoryDetailComponent } from './category-detail/category-detail.component';

// Guards
import { AuthGuard } from '../../core/guards/auth.guard';

const routes: Routes = [
  { path: '', component: CategoryListComponent },
  { path: ':id', component: CategoryDetailComponent }
];

@NgModule({
  declarations: [
    CategoryListComponent,
    CategoryDetailComponent
  ],
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatPaginatorModule
  ],
  exports: [
    CategoryListComponent,
    CategoryDetailComponent
  ]
})
export class CategoriesModule { }