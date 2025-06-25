import { User } from './user.model';
import { Category } from './category.model';
import { Comment } from './comment.model';

export interface Article {
  id: number;
  title: string;
  content: string;
  summary?: string;
  createdAt: Date;
  updatedAt?: Date;
  authorId: number;
  author?: User;
  categories?: Category[];
  comments?: Comment[];
  imagePath?: string;
  commentCount?: number;
}

export interface ArticleCreate {
  title: string;
  content: string;
  categoryIds: number[];
  // Image will be handled separately as a file upload
}

export interface ArticleUpdate {
  title: string;
  content: string;
  categoryIds: number[];
  // Image will be handled separately as a file upload
  removeExistingImage?: boolean;
}

export interface ArticlePagination {
  articles: Article[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
}

export interface SearchResult {
  results: Article[]; // Changed from 'articles' to 'results' to match backend API
  totalCount: number;
  searchTerm: string;
}