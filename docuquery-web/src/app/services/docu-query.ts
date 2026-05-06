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
  sessionId: string;
  history: ChatTurn[];
}

export interface ChatTurn {
  role: string;
  content: string;
}

export interface ChatResult {
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
  readonly sessionId = crypto.randomUUID();

  constructor(private http: HttpClient) {}

  uploadDocument(file: File): Observable<IngestResponse> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('sessionId', this.sessionId);
    return this.http.post<IngestResponse>(`${this.apiUrl}/api/documents/upload`, formData);
  }

  askQuestion(question: string, history: ChatTurn[] = []): Observable<ChatResult> {
    const request: ChatRequest = { question, sessionId: this.sessionId, history };
    return this.http.post<ChatResult>(`${this.apiUrl}/api/chat/ask`, request);
  }
}