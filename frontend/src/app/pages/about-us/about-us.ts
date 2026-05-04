import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-about-us',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './about-us.html',
  styleUrls: ['./about-us.css']
})
export class AboutUs {
  constructor(private router: Router) {}

  navigateToHome() {
    this.router.navigate(['/']);
  }
}
