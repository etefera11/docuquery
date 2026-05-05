import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DocuQueryService, ChatTurn, ChatResponse } from '../services/docu-query';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.html',
  styleUrl: './chat.scss'
})
export class ChatComponent {
  question = '';
  asking = false;
  history: ChatTurn[] = [];
  messages: { role: string; content: string }[] = [];
  error: string | null = null;

  constructor(private docuQuery: DocuQueryService) {}

  ask() {
    if (!this.question.trim() || this.asking) return;

    const userQuestion = this.question.trim();
    this.question = '';
    this.asking = true;
    this.error = null;

    this.messages.push({ role: 'user', content: userQuestion });

    this.docuQuery.askQuestion(userQuestion, this.history).subscribe({
      next: (response: ChatResponse) => {
        this.messages.push({ role: 'assistant', content: response.answer });
        this.history.push({ role: 'user', content: userQuestion });
        this.history.push({ role: 'assistant', content: response.answer });
        this.asking = false;
      },
      error: (err) => {
        this.error = 'Failed to get answer. Please try again.';
        this.asking = false;
        console.error(err);
      }
    });
  }

  onKeyDown(event: KeyboardEvent) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.ask();
    }
  }

  clearChat() {
    this.messages = [];
    this.history = [];
    this.error = null;
  }
}