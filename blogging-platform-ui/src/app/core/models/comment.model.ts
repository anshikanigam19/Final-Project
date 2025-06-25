import { User } from './user.model';

export interface Comment {
  id: number;
  content: string;
  createdAt: Date;
  articleId: number;
  articleTitle?: string;
  userId: number;
  user?: User;
}

export interface CommentCreate {
  content: string;
  articleId: number;
}