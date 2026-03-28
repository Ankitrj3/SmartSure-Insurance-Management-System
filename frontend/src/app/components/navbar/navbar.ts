import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, NavigationEnd } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { CommonModule } from '@angular/common';
import { Subscription, filter } from 'rxjs';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar implements OnInit, OnDestroy {
  isLoggedIn = false;
  isAdmin = false;
  showDropdown = false;
  private subs = new Subscription();

  constructor(
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    // Re-check auth state on every completed navigation (covers login redirect)
    this.subs.add(
      this.router.events
        .pipe(filter(e => e instanceof NavigationEnd))
        .subscribe(() => {
          this.updateAuthState();
        })
    );

    // Also react immediately to explicit login/logout calls
    this.subs.add(
      this.authService.authStatus$.subscribe(() => {
        this.updateAuthState();
      })
    );
  }

  private updateAuthState() {
    this.isLoggedIn = this.authService.hasToken();
    this.isAdmin = this.authService.getRole()?.toLowerCase() === 'admin';
    this.cdr.detectChanges();
  }

  ngOnDestroy() {
    this.subs.unsubscribe();
  }

  toggleDropdown(event: Event) {
    event.stopPropagation();
    this.showDropdown = !this.showDropdown;
    
    if (this.showDropdown) {
      setTimeout(() => {
        document.addEventListener('click', this.closeDropdownOnClickOutside);
      }, 0);
    }
  }

  closeDropdown() {
    this.showDropdown = false;
    document.removeEventListener('click', this.closeDropdownOnClickOutside);
  }

  private closeDropdownOnClickOutside = () => {
    this.showDropdown = false;
    this.cdr.detectChanges();
    document.removeEventListener('click', this.closeDropdownOnClickOutside);
  }

  logout() {
    this.closeDropdown();
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
