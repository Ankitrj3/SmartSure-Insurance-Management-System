import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-contact-us',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './contact-us.html',
  styleUrls: ['./contact-us.css']
})
export class ContactUs {
  constructor(private router: Router) {}

  navigateToHome() {
    this.router.navigate(['/']);
  }
}
