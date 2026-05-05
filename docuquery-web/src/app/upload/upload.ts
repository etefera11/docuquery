import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DocuQueryService, IngestResponse } from '../services/docu-query';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-upload',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatProgressBarModule, MatCardModule],
  templateUrl: './upload.html',
  styleUrl: './upload.scss'
})
export class UploadComponent {
  selectedFile: File | null = null;
  uploading = false;
  result: IngestResponse | null = null;
  error: string | null = null;

  constructor(private docuQuery: DocuQueryService) {}

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      this.selectedFile = input.files[0];
      this.result = null;
      this.error = null;
    }
  }

  upload() {
    if (!this.selectedFile) return;

    this.uploading = true;
    this.error = null;

    this.docuQuery.uploadDocument(this.selectedFile).subscribe({
      next: (response) => {
        this.result = response;
        this.uploading = false;
      },
      error: (err) => {
        this.error = 'Upload failed. Please try again.';
        this.uploading = false;
        console.error(err);
      }
    });
  }
}