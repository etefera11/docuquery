import { Component } from '@angular/core';
import { UploadComponent } from './upload/upload';
import { ChatComponent } from './chat/chat';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [UploadComponent, ChatComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class AppComponent {
  title = 'docuquery-web';
}