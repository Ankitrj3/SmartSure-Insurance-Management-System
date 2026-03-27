import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { PolicyService } from '../../core/services/policy.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class Home implements OnInit {
  isLoading = signal(true);
  insuranceTypes = signal<any[]>([]);
  insuranceSubtypes = signal<any[]>([]);
  showAllVehicleSubtypes = signal(false);
  showAllHomeSubtypes = signal(false);

  constructor(
    private policyService: PolicyService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    console.log('HOME COMPONENT INITIALIZED - SIGNAL VERSION');
    this.loadData();
  }

  get isLoggedIn(): boolean {
    return this.authService.hasToken();
  }

  loadData() {
    this.isLoading.set(true);
    console.log('Loading data...');
    
    this.policyService.getInsuranceTypes().subscribe({
      next: (response: any) => {
        console.log('Types response:', response);
        
        let types = response;
        if (response?.data) types = response.data;
        if (types?.$values) types = types.$values;
        
        const typesArray = Array.isArray(types) ? types : [];
        this.insuranceTypes.set(typesArray);
        console.log('Types set:', typesArray.length);
        
        this.policyService.getInsuranceSubtypes().subscribe({
          next: (response2: any) => {
            console.log('Subtypes response:', response2);
            
            let subtypes = response2;
            if (response2?.data) subtypes = response2.data;
            if (subtypes?.$values) subtypes = subtypes.$values;
            
            const subtypesArray = Array.isArray(subtypes) ? subtypes : [];
            this.insuranceSubtypes.set(subtypesArray);
            console.log('Subtypes set:', subtypesArray.length);
            
            this.isLoading.set(false);
            console.log('Loading complete!');
          },
          error: (err) => {
            console.error('Subtypes error:', err);
            this.isLoading.set(false);
          }
        });
      },
      error: (err) => {
        console.error('Types error:', err);
        this.isLoading.set(false);
      }
    });
  }

  getSubtypesForType(typeId: string) {
    return this.insuranceSubtypes().filter(s => 
      (s.typeId || s.TypeId) === typeId
    );
  }

  getVehicleType() {
    return this.insuranceTypes().find(t => 
      (t.name || t.Name)?.toLowerCase() === 'vehicle'
    );
  }

  getHomeType() {
    return this.insuranceTypes().find(t => 
      (t.name || t.Name)?.toLowerCase() === 'home'
    );
  }

  getVehicleSubtypes() {
    const vehicleType = this.getVehicleType();
    if (!vehicleType) return [];
    const subtypes = this.getSubtypesForType(vehicleType.typeId || vehicleType.TypeId);
    return this.showAllVehicleSubtypes() ? subtypes : subtypes.slice(0, 6);
  }

  getHomeSubtypes() {
    const homeType = this.getHomeType();
    if (!homeType) return [];
    const subtypes = this.getSubtypesForType(homeType.typeId || homeType.TypeId);
    return this.showAllHomeSubtypes() ? subtypes : subtypes.slice(0, 6);
  }

  toggleVehicleSubtypes() {
    this.showAllVehicleSubtypes.set(!this.showAllVehicleSubtypes());
  }

  toggleHomeSubtypes() {
    this.showAllHomeSubtypes.set(!this.showAllHomeSubtypes());
  }

  hasMoreVehicleSubtypes() {
    const vehicleType = this.getVehicleType();
    if (!vehicleType) return false;
    return this.getSubtypesForType(vehicleType.typeId || vehicleType.TypeId).length > 6;
  }

  hasMoreHomeSubtypes() {
    const homeType = this.getHomeType();
    if (!homeType) return false;
    return this.getSubtypesForType(homeType.typeId || homeType.TypeId).length > 6;
  }

  onPurchaseClick() {
    if (this.isLoggedIn) {
      this.router.navigate(['/user-dashboard']);
    } else {
      this.router.navigate(['/login']);
    }
  }

  onHeroAction() {
    if (this.isLoggedIn) {
      this.router.navigate(['/user-dashboard']);
    } else {
      this.router.navigate(['/login']);
    }
  }

  getIconClass(name: string): string {
    const lowerName = name?.toLowerCase() || '';
    if (lowerName.includes('car') || lowerName.includes('auto') || lowerName.includes('vehicle')) {
      return 'icon-car';
    } else if (lowerName.includes('home') || lowerName.includes('house') || lowerName.includes('property')) {
      return 'icon-home';
    } else if (lowerName.includes('health') || lowerName.includes('medical') || lowerName.includes('life')) {
      return 'icon-health';
    } else if (lowerName.includes('travel') || lowerName.includes('trip')) {
      return 'icon-travel';
    }
    return 'icon-default';
  }

  isCarInsurance(name: string): boolean {
    const lowerName = name?.toLowerCase() || '';
    return lowerName.includes('car') || lowerName.includes('auto') || lowerName.includes('vehicle');
  }

  isHomeInsurance(name: string): boolean {
    const lowerName = name?.toLowerCase() || '';
    return lowerName.includes('home') || lowerName.includes('house') || lowerName.includes('property');
  }

  isHealthInsurance(name: string): boolean {
    const lowerName = name?.toLowerCase() || '';
    return lowerName.includes('health') || lowerName.includes('medical') || lowerName.includes('life');
  }

  isTravelInsurance(name: string): boolean {
    const lowerName = name?.toLowerCase() || '';
    return lowerName.includes('travel') || lowerName.includes('trip');
  }
}
