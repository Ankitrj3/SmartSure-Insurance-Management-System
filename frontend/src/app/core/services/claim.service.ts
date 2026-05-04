import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ClaimService {
  constructor(private api: ApiService) {}

  submitClaim(claimData: any): Observable<any> {
    return this.api.post<any>('claims', claimData);
  }

  updateClaim(claimId: string, claimData: any): Observable<any> {
    return this.api.put<any>(`claims/${claimId}`, claimData);
  }

  submitClaimToReview(claimId: string): Observable<any> {
    return this.api.put<any>(`claims/${claimId}/submit`, {});
  }

  getUserClaims(): Observable<any[]> {
    return this.api.get<any[]>('claims?page=1&pageSize=3000');
  }

  getClaimDetails(claimId: string): Observable<any> {
    return this.api.get<any>(`claims/${claimId}`);
  }

  getClaimsByPolicy(policyId: string): Observable<any[]> {
    return this.api.get<any[]>(`claims/by-policy/${policyId}`);
  }

  // Documents
  getClaimDocuments(claimId: string): Observable<any[]> {
    return this.api.get<any[]>(`claims/${claimId}/documents`);
  }

  /**
   * File uploads must use postFormData – not post – so the browser
   * can set the correct multipart/form-data boundary automatically.
   */
  uploadDocument(claimId: string, formData: FormData): Observable<any> {
    return this.api.postFormData<any>(`claims/${claimId}/documents`, formData);
  }

  deleteClaimDocument(claimId: string, docId: string): Observable<any> {
    return this.api.delete<any>(`claims/${claimId}/documents/${docId}`);
  }

  getClaimHistory(claimId: string): Observable<any[]> {
    return this.api.get<any[]>(`claims/${claimId}/history`);
  }
}
