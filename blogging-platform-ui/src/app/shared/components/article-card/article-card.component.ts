import { Component, Input, OnInit } from '@angular/core';
import { Article } from '../../../core/models/article.model';

@Component({
  selector: 'app-article-card',
  templateUrl: './article-card.component.html',
  styleUrls: ['./article-card.component.scss']
})
export class ArticleCardComponent implements OnInit {
  @Input() article!: Article;
  @Input() showFullContent = false;

  constructor() { }

  ngOnInit(): void {
  }

  getArticleExcerpt(content: string): string {
    if (!content) return '';
    // Strip HTML tags and get plain text
    const plainText = content.replace(/<[^>]*>/g, '');
    // Return first 150 characters
    return plainText.length > 150 ? plainText.substring(0, 150) + '...' : plainText;
  }
}