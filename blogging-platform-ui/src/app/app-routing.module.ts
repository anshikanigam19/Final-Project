import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Components
import { HomeComponent } from './features/home/home.component';
import { PageNotFoundComponent } from './core/components/page-not-found/page-not-found.component';

// Guards
import { AuthGuard } from './core/guards/auth.guard';

const routes: Routes = [
  { path: '', component: HomeComponent },
  { 
    path: 'auth', 
    loadChildren: () => import('./features/auth/auth.module').then(m => m.AuthModule) 
  },
  { 
    path: 'articles', 
    loadChildren: () => import('./features/articles/articles.module').then(m => m.ArticlesModule) 
  },
  { 
    path: 'categories', 
    loadChildren: () => import('./features/categories/categories.module').then(m => m.CategoriesModule) 
  },
  { 
    path: 'profile', 
    loadChildren: () => import('./features/profile/profile.module').then(m => m.ProfileModule),
    canActivate: [AuthGuard]
  },
  { 
    path: 'search', 
    loadChildren: () => import('./features/search/search.module').then(m => m.SearchModule) 
  },
  { path: '**', component: PageNotFoundComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }