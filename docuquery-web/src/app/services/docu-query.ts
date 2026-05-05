import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface IngestResponse {
  fileName: string;
  chunksCreated: number;
}

export interface ChatRequest {
  question: string;
  history: ChatTurn[];
}

export interface ChatTurn {
  role: string;
  content: string;
}

export interface ChatResponse {
  answer: string;
  citations: Citation[];
}

export interface Citation {
  fileName: string;
  pageNumber: number;
  excerpt: string;
}

@Injectable({
  providedIn: 'root'
})
export class DocuQueryService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  uploadDocument(file: File): Observable<IngestResponse> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<IngestResponse>(`${this.apiUrl}/api/documents/upload`, formData);
  }

  askQuestion(question: string, history: ChatTurn[] = []): Observable<ChatResponse> {
    const request: ChatRequest = { question, history };
    return this.http.post<ChatResponse>(`${this.apiUrl}/api/chat/ask`, request);
  }
}