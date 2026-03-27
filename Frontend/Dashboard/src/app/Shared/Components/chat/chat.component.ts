import { Component, ViewChild, ElementRef, AfterViewChecked, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../../Core/Services/chat.service';
import {marked} from 'marked';

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
  selectedProvider: number = 0; // 0 = Gemini, 1 = Claude
  // messages: Message[] = [
  //   { text: 'Hello! How can I help you grow in the AI world today?', sender: 'ai', time: new Date() }
  // ];

  private readonly STORAGE_KEY = 'chat_messages';

  messages: Message[] = this.loadMessages();

  constructor(private chatService: ChatService,
              private cd: ChangeDetectorRef
  ) {}

  toggleChat() {
    this.isOpen = !this.isOpen;
  }

  sendMessage() {
    if (this.newMessage.trim() && !this.isLoading) {
      this.messages.push({
        text: this.newMessage,
        sender: 'user',
        time: new Date()
      });
      this.saveMessages(); // ✅ save user message

      const userText = this.newMessage;
      this.newMessage = '';
      this.isLoading = true;

      this.chatService.sendMessage(userText, this.selectedProvider).subscribe({
        next: (res) => {
          this.messages.push({
            text: res.answer,
            sender: 'ai',
            time: new Date()
          });
          this.saveMessages(); // ✅ save AI response
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
          this.saveMessages(); // ✅ save error message too
          this.isLoading = false;
        }
      });
    }
  }

  private loadMessages(): Message[] {
    const saved = localStorage.getItem(this.STORAGE_KEY);
    if (saved) {
      const parsed = JSON.parse(saved);
      // Convert time strings back to Date objects
      return parsed.map((m: any) => ({ ...m, time: new Date(m.time) }));
    }
    return [
      { text: 'Hello! How can I help you grow in the AI world today?', sender: 'ai', time: new Date() }
    ];
  }

  private saveMessages(): void {
    localStorage.setItem(this.STORAGE_KEY, JSON.stringify(this.messages));
  }

  clearMessages(): void {
    localStorage.removeItem(this.STORAGE_KEY);
    this.messages = [
      { text: 'Hello! How can I help you grow in the AI world today?', sender: 'ai', time: new Date() }
    ];
  }

  formatMessage(text: string): string {
    return marked.parse(text) as string;
  }
}

