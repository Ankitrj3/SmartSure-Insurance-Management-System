import { Component, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, CommonModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar implements OnInit {
  isLoggedIn = false;
  isAdmin = false;

  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit() {
    this.authService.authStatus$.subscribe(status => {
      this.isLoggedIn = status;
      this.isAdmin = this.authService.getRole()?.toLowerCase() === 'admin';
    });
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
