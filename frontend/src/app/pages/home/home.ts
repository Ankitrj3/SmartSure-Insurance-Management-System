import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { PolicyService } from '../../core/services/policy.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home implements OnInit {
  insuranceTypes: any[] = [];
  insuranceSubtypes: any[] = [];
  loading = true;

  constructor(
    private policyService: PolicyService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    this.fetchPublicPlans();
  }

  get isLoggedIn(): boolean {
    return this.authService.hasToken();
  }

  fetchPublicPlans() {
    this.policyService.getInsuranceTypes().subscribe({
      next: (typesData: any) => {
        // Handle potential $values wrapper from Newtonsoft.Json
        this.insuranceTypes = this.extractData(typesData);
        
        this.policyService.getInsuranceSubtypes().subscribe({
          next: (subtypesData: any) => {
            const allSubtypes = this.extractData(subtypesData);
            
            this.insuranceSubtypes = allSubtypes.map(s => {
              // Be robust to property naming (camelCase vs PascalCase)
              const sTypeId = s.typeId || s.TypeId || s.insuranceTypeId || s.InsuranceTypeId;
              const parent = this.insuranceTypes.find(t => {
                 const tId = t.typeId || t.TypeId || t.id || t.Id;
                 return tId === sTypeId;
              });
              return { 
                ...s, 
                id: s.subtypeId || s.SubtypeId || s.id,
                name: s.name || s.Name,
                description: s.description || s.Description,
                typeName: parent?.name || parent?.Name || 'Insurance Plan' 
              };
            });
            this.loading = false;
          },
          error: (err) => {
            console.error('Error fetching subtypes', err);
            this.loading = false;
          }
        });
      },
      error: (err) => {
        console.error('Error fetching types', err);
        this.loading = false;
      }
    });
  }

  private extractData(data: any): any[] {
    if (Array.isArray(data)) return data;
    if (data && data.$values && Array.isArray(data.$values)) return data.$values;
    return [];
  }

  getSubtypesForType(type: any) {
    const typeId = type.typeId || type.TypeId || type.id || type.Id;
    return this.insuranceSubtypes.filter(s => {
       const sTypeId = s.typeId || s.TypeId || s.insuranceTypeId || s.InsuranceTypeId;
       return sTypeId === typeId;
    });
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
}
