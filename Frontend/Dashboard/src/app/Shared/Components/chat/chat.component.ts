import { Component, ViewChild, ElementRef, AfterViewChecked, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../../Core/Services/chat.service';

interface Message {
  text: string;
  sender: 'user' | 'ai';
  time: Date;
}

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.scss'
})
export class ChatComponent {
  @ViewChild('messagesContainer') private messagesContainer!: ElementRef;

  isOpen = false;
  isLoading = false;
  newMessage = '';
  messages: Message[] = [
    { text: 'Hello! How can I help you grow in the AI world today?', sender: 'ai', time: new Date() }
  ];

  constructor(private chatService: ChatService,
              private cd: ChangeDetectorRef
  ) {}

  toggleChat() {
    this.isOpen = !this.isOpen;
  }

  sendMessage() {
    if (this.newMessage.trim() && !this.isLoading) {
      // Add user message
      this.messages.push({
        text: this.newMessage,
        sender: 'user',
        time: new Date()
      });

      const userText = this.newMessage;
      this.newMessage = '';
      this.isLoading = true;

      // Call backend AI service
      this.chatService.sendMessage(userText).subscribe({
        next: (response) => {
          this.messages.push({
            text: response.response,
            sender: 'ai',
            time: new Date()
          });
          this.isLoading = false;
          this.cd.detectChanges();
        },
        error: (err) => {
          console.error('Chat error:', err);
          this.messages.push({
            text: 'Sorry, I encountered an error. Please try again.',
            sender: 'ai',
            time: new Date()
          });
          this.isLoading = false;
        }
      });
    }
  }
}

