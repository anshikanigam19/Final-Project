import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-loading-spinner',
  templateUrl: './loading-spinner.component.html',
  styleUrls: ['./loading-spinner.component.scss']
})
export class LoadingSpinnerComponent {
  @Input() diameter = 40;
  @Input() color: 'primary' | 'accent' | 'warn' = 'primary';
  @Input() message = 'Loading...';
  @Input() showMessage = true;

  constructor() { }
}