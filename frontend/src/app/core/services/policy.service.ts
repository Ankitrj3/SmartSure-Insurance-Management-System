import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PolicyService {
  constructor(private api: ApiService) {}

  // Insurance Types & Subtypes
  getInsuranceTypes(): Observable<any[]> {
    return this.api.get<any[]>('insurance-types');
  }

  getInsuranceTypeById(typeId: string): Observable<any> {
    return this.api.get<any>(`insurance-types/${typeId}`);
  }

  createInsuranceType(data: any): Observable<any> {
    return this.api.post<any>('insurance-types', data);
  }

  updateInsuranceType(typeId: string, data: any): Observable<any> {
    return this.api.put<any>(`insurance-types/${typeId}`, data);
  }

  deleteInsuranceType(typeId: string): Observable<any> {
    return this.api.delete<any>(`insurance-types/${typeId}`);
  }

  getInsuranceSubtypes(): Observable<any[]> {
    return this.api.get<any[]>('insurance-subtypes');
  }

  createInsuranceSubtype(data: any): Observable<any> {
    return this.api.post<any>('insurance-subtypes', data);
  }

  updateInsuranceSubtype(subtypeId: string, data: any): Observable<any> {
    return this.api.put<any>(`insurance-subtypes/${subtypeId}`, data);
  }

  deleteInsuranceSubtype(subtypeId: string): Observable<any> {
    return this.api.delete<any>(`insurance-subtypes/${subtypeId}`);
  }

  getSubtypesByType(typeId: string): Observable<any[]> {
    return this.api.get<any[]>(`insurance-types/${typeId}/subtypes`);
  }

  // Policies
  getUserPolicies(): Observable<any[]> {
    return this.api.get<any[]>('policies');
  }

  getPolicyById(policyId: string): Observable<any> {
    return this.api.get<any>(`policies/${policyId}`);
  }

  cancelPolicy(policyId: string): Observable<any> {
    return this.api.put<any>(`policies/${policyId}/cancel`, {});
  }

  getPolicyDetails(policyId: string): Observable<any> {
    return this.api.get<any>(`policies/${policyId}/details`);
  }

  savePolicyDetails(policyId: string, data: any): Observable<any> {
    return this.api.post<any>(`policies/${policyId}/details`, data);
  }

  updatePolicyDetails(policyId: string, data: any): Observable<any> {
    return this.api.put<any>(`policies/${policyId}/details`, data);
  }

  createHomePolicy(data: any): Observable<any> {
    return this.api.post<any>('policies', data); 
  }

  createVehiclePolicy(data: any): Observable<any> {
    return this.api.post<any>('policies', data); 
  }

  getHomeDetail(policyId: string): Observable<any> {
    return this.api.get<any>(`home-details/${policyId}`);
  }

  saveHomeDetail(data: any): Observable<any> {
    return this.api.post<any>('home-details', data);
  }

  getVehicleDetail(policyId: string): Observable<any> {
    return this.api.get<any>(`vehicle-details/${policyId}`);
  }

  saveVehicleDetail(data: any): Observable<any> {
    return this.api.post<any>('vehicle-details', data);
  }

  getPremium(policyId: string): Observable<any> {
    return this.api.get<any>(`policies/${policyId}/premium`);
  }

  // Payments
  getPayments(policyId: string): Observable<any[]> {
    return this.api.get<any[]>(`policies/${policyId}/payments`);
  }

  getPaymentDetails(paymentId: string): Observable<any> {
    return this.api.get<any>(`payments/${paymentId}`);
  }

  processPayment(policyId: string, paymentDetails: any): Observable<any> {
    return this.api.post<any>(`policies/${policyId}/payments`, paymentDetails);
  }
}
