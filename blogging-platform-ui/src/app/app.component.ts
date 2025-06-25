import { Component, OnInit } from '@angular/core';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'Modern Blogging Platform';

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    // Check if user is already logged in (token in localStorage)
    this.authService.autoLogin();
  }
}