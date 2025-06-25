import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';

// Material Imports
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatChipsModule } from '@angular/material/chips';
import { MatSelectModule } from '@angular/material/select';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';

// Components
import { ArticleListComponent } from './article-list/article-list.component';
import { ArticleDetailComponent } from './article-detail/article-detail.component';
import { ArticleCreateComponent } from './article-create/article-create.component';
import { ArticleEditComponent } from './article-edit/article-edit.component';

// Guards
import { AuthGuard } from '../../core/guards/auth.guard';

// Editor Module
import { EditorModule } from '@tinymce/tinymce-angular';

const routes: Routes = [
  { path: '', component: ArticleListComponent },
  { path: 'create', component: ArticleCreateComponent, canActivate: [AuthGuard] },
  { path: 'edit/:id', component: ArticleEditComponent, canActivate: [AuthGuard] },
  { path: ':id', component: ArticleDetailComponent }
];

@NgModule({
  declarations: [
    ArticleListComponent,
    ArticleDetailComponent,
    ArticleCreateComponent,
    ArticleEditComponent
  ],
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatProgressSpinnerModule,
    MatPaginatorModule,
    MatChipsModule,
    MatSelectModule,
    MatDividerModule,
    MatListModule,
    EditorModule
  ],
  exports: [
    ArticleListComponent,
    ArticleDetailComponent,
    ArticleCreateComponent,
    ArticleEditComponent
  ]
})
export class ArticlesModule { }