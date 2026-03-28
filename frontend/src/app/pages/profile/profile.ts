import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile implements OnInit {
  profile: any = {};
  loading = true;
  passwordData = { currentPassword: '', newPassword: '' };
  profileMessage = '';
  passwordMessage = '';
  isEditingProfile = false;
  isEditingPassword = false;

  constructor(
    private authService: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.fetchProfile();
  }

  fetchProfile() {
    this.loading = true;
    this.profileMessage = '';
    
    this.authService.getProfile().pipe(
      catchError((error) => {
        console.error('Profile fetch error:', error);
        
        if (error.status === 0) {
          this.profileMessage = 'Cannot connect to server. Please ensure the backend is running.';
        } else if (error.status === 401) {
          this.profileMessage = 'Session expired. Please login again.';
        } else if (error.status === 404) {
          this.profileMessage = 'Profile endpoint not found. Please check API configuration.';
        } else {
          this.profileMessage = `Failed to load profile: ${error.message || 'Unknown error'}`;
        }
        
        return of(null);
      })
    ).subscribe({
      next: (data: any) => {
        if (data) {
          console.log('Profile data received:', data);
          // Handle both camelCase and PascalCase from API
          this.profile = {
            userId: data.userId || data.UserId || '',
            fullName: data.fullName || data.FullName || '',
            phoneNumber: data.phoneNumber || data.PhoneNumber || '',
            address: data.address || data.Address || '',
            email: data.email || data.Email || ''
          };
          console.log('Profile object set:', this.profile);
        }
        // Always set loading to false, whether data exists or not
        this.loading = false;
        this.cdr.detectChanges(); // Force change detection
      },
      error: () => {
        // Fallback in case error isn't caught
        this.loading = false;
        this.cdr.detectChanges(); // Force change detection
      }
    });
  }

  enableEditProfile() {
    this.isEditingProfile = true;
    this.profileMessage = '';
  }

  cancelEditProfile() {
    this.isEditingProfile = false;
    this.profileMessage = '';
    this.fetchProfile(); // Reset to original values
  }

  updateProfile() {
    if (!this.profile.fullName || !this.profile.fullName.trim()) {
      this.profileMessage = 'Please enter your full name.';
      return;
    }
    
    if (!this.profile.phoneNumber || !this.profile.phoneNumber.trim()) {
      this.profileMessage = 'Please enter your phone number.';
      return;
    }
    
    // Send data with PascalCase to match C# DTO
    const updateData = { 
      FullName: this.profile.fullName.trim(),
      PhoneNumber: this.profile.phoneNumber.trim(),
      Address: this.profile.address?.trim() || ''
    };
    
    console.log('=== UPDATE PROFILE ===');
    console.log('Current profile state:', this.profile);
    console.log('Sending update data:', updateData);
    console.log('JSON stringified:', JSON.stringify(updateData));
    
    this.authService.updateProfile(updateData).subscribe({
      next: (response) => {
        console.log('Update response:', response);
        this.profileMessage = 'Success: Profile updated!';
        this.isEditingProfile = false;
        
        // Wait a bit before fetching to ensure backend has saved
        setTimeout(() => {
          this.fetchProfile();
        }, 500);
      },
      error: (err) => {
        console.error('Update error:', err);
        console.error('Error details:', err.error);
        this.profileMessage = 'Failed to update: ' + (err.error?.message || err.message);
      }
    });
  }

  enableEditPassword() {
    this.isEditingPassword = true;
    this.passwordMessage = '';
  }

  cancelEditPassword() {
    this.isEditingPassword = false;
    this.passwordMessage = '';
    this.passwordData = { currentPassword: '', newPassword: '' };
  }

  changePassword() {
    if (!this.passwordData.currentPassword || !this.passwordData.newPassword) {
      this.passwordMessage = 'Please fill in both password fields.';
      return;
    }
    
    this.authService.changePassword(this.passwordData).subscribe({
      next: () => {
        this.passwordMessage = 'Success: Password changed.';
        this.passwordData = { currentPassword: '', newPassword: '' };
        this.isEditingPassword = false;
      },
      error: (err) => this.passwordMessage = 'Failed to change password: ' + (err.error?.message || err.message)
    });
  }
}
