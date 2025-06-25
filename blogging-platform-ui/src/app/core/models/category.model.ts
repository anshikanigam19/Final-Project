export interface Category {
  id: number;
  name: string;
  description?: string;
}

export interface CategoryWithCount extends Category {
  articleCount: number;
}