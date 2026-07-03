import {
  Component,
  ChangeDetectorRef,
  HostListener,
  OnDestroy
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { ChatService } from '../../../Core/Services/chat.service';
import { parseProblemDetails } from '../../../Core/Models/problem-details';
import { marked } from 'marked';

interface ChatThread {
  threadId: string;
  title: string;
  lastMessageAt: Date;
}

interface Message {
  text: string;
  sender: 'user' | 'ai';
  time: Date;
}

type WindowSize = 'compact' | 'expanded' | 'maximized' | 'custom';

interface SizeState {
  preset: WindowSize;
  width: number | null;
  height: number | null;
}

const SIZE_STORAGE_KEY = 'chat_window_size';

const PRESETS: Record<Exclude<WindowSize, 'custom'>, { w: number; h: number }> = {
  compact:    { w: 380,  h: 600 },
  expanded:   { w: 560,  h: 720 },
  maximized:  { w: 960,  h: 900 }
};

const MIN_WIDTH = 320;
const MIN_HEIGHT = 360;

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.scss'
})
export class ChatComponent implements OnDestroy {
  threads: ChatThread[] = [];
  currentThreadId: string | null = null;
  showThreadList: boolean = false;

  isOpen = false;
  isLoading = false;
  newMessage = '';
  selectedProvider: number = 0; // 0 = Gemini, 1 = Claude
  messages: Message[] = [];

  windowSize: WindowSize = 'compact';
  customWidth: number | null = null;
  customHeight: number | null = null;
  isResizing = false;

  private resizeStartX = 0;
  private resizeStartY = 0;
  private resizeStartWidth = 0;
  private resizeStartHeight = 0;

  constructor(private chatService: ChatService,
              private cd: ChangeDetectorRef
  ) {
    this.loadThreadList();
    this.loadSizeState();
  }

  ngOnDestroy(): void {
    this.endResize();
  }

  loadThreadList() {
    this.chatService.getThreads().subscribe({
      next: (threads) => {
        this.threads = threads.map(t => ({
          threadId: t.threadId,
          title: t.title,
          lastMessageAt: new Date(t.lastMessageAt)
        }));

        if (this.threads.length > 0) {
          this.switchThread(this.threads[0].threadId);
        } else {
          this.showGreeting();
        }
        this.cd.detectChanges();
      },
      error: () => {
        this.threads = [];
        this.showGreeting();
      }
    });
  }

  private showGreeting(): void {
    this.messages = [
      {
        text: 'Hello! How can I help you grow in the AI world today?',
        sender: 'ai',
        time: new Date()
      }
    ];
  }

  switchThread(threadId: string): void {
    this.currentThreadId = threadId;
    this.showThreadList = false;
    this.loadMessages(threadId);
  }

  toggleChat() {
    this.isOpen = !this.isOpen;
  }

  /* ===== Window sizing ===== */

  cycleSize(): void {
    const order: WindowSize[] = ['compact', 'expanded', 'maximized'];
    const start = order.indexOf(this.windowSize);
    const next = order[(start + 1) % order.length];
    this.windowSize = next;
    this.customWidth = null;
    this.customHeight = null;
    this.saveSizeState();
  }

  get effectiveWidth(): number {
    if (this.windowSize === 'custom' && this.customWidth) return this.customWidth;
    const preset = PRESETS[this.windowSize as Exclude<WindowSize, 'custom'>] ?? PRESETS.compact;
    return Math.min(preset.w, window.innerWidth - 32);
  }

  get effectiveHeight(): number {
    if (this.windowSize === 'custom' && this.customHeight) return this.customHeight;
    const preset = PRESETS[this.windowSize as Exclude<WindowSize, 'custom'>] ?? PRESETS.compact;
    return Math.min(preset.h, window.innerHeight - 100);
  }

  get sizeButtonTitle(): string {
    switch (this.windowSize) {
      case 'compact':   return 'Expand chat';
      case 'expanded':  return 'Maximize chat';
      case 'maximized': return 'Reset to compact';
      default:          return 'Resize chat';
    }
  }

  startResize(event: MouseEvent | TouchEvent): void {
    event.preventDefault();
    event.stopPropagation();
    const point = this.pointFromEvent(event);
    if (!point) return;

    this.isResizing = true;
    this.resizeStartX = point.x;
    this.resizeStartY = point.y;
    this.resizeStartWidth = this.effectiveWidth;
    this.resizeStartHeight = this.effectiveHeight;
  }

  @HostListener('document:mousemove', ['$event'])
  @HostListener('document:touchmove', ['$event'])
  onResizeMove(event: MouseEvent | TouchEvent): void {
    if (!this.isResizing) return;
    const point = this.pointFromEvent(event);
    if (!point) return;

    // Dragging the top-left handle: moving up/left makes window bigger.
    const dx = this.resizeStartX - point.x;
    const dy = this.resizeStartY - point.y;

    const newWidth  = Math.max(MIN_WIDTH,  Math.min(this.resizeStartWidth  + dx, window.innerWidth - 32));
    const newHeight = Math.max(MIN_HEIGHT, Math.min(this.resizeStartHeight + dy, window.innerHeight - 32));

    this.windowSize = 'custom';
    this.customWidth = newWidth;
    this.customHeight = newHeight;
  }

  @HostListener('document:mouseup')
  @HostListener('document:touchend')
  @HostListener('document:touchcancel')
  onResizeEnd(): void {
    if (!this.isResizing) return;
    this.endResize();
    this.saveSizeState();
  }

  private endResize(): void {
    this.isResizing = false;
  }

  private pointFromEvent(event: MouseEvent | TouchEvent): { x: number; y: number } | null {
    if (event instanceof TouchEvent) {
      const t = event.touches[0] ?? event.changedTouches[0];
      return t ? { x: t.clientX, y: t.clientY } : null;
    }
    return { x: (event as MouseEvent).clientX, y: (event as MouseEvent).clientY };
  }

  private loadSizeState(): void {
    try {
      const raw = localStorage.getItem(SIZE_STORAGE_KEY);
      if (!raw) return;
      const state = JSON.parse(raw) as SizeState;
      this.windowSize = state.preset ?? 'compact';
      this.customWidth = state.width ?? null;
      this.customHeight = state.height ?? null;
    } catch { /* ignore */ }
  }

  private saveSizeState(): void {
    const state: SizeState = {
      preset: this.windowSize,
      width: this.customWidth,
      height: this.customHeight
    };
    try { localStorage.setItem(SIZE_STORAGE_KEY, JSON.stringify(state)); } catch { /* ignore */ }
  }

  /* ===== Messaging ===== */

  sendMessage() {
    if (!this.newMessage.trim() || this.isLoading) return;
    this.messages.push({
        text: this.newMessage,
        sender: 'user',
        time: new Date()
    });

    const userText = this.newMessage;
    this.newMessage = '';
    this.isLoading = true;

    this.chatService.sendMessage(userText,
      this.selectedProvider, this.currentThreadId ?? undefined).subscribe({
        next: (res) => {
          if (!this.currentThreadId) {
            this.currentThreadId = res.threadId;
            this.threads.unshift({
              threadId: res.threadId,
              title: userText.length > 40 ? userText.substring(0, 40) + '...' : userText,
              lastMessageAt: new Date()
            });
          }
          else {
            const thread = this.threads.find(t => t.threadId == this.currentThreadId);
            if (thread) {
              thread.lastMessageAt = new Date();
            }
          }
          this.messages.push({
            text: res.answer,
            sender: 'ai',
            time: new Date()
          });
          this.isLoading = false;
          this.cd.detectChanges();
        },
        error: (err: HttpErrorResponse) => {
          this.messages.push({
            text: this.buildErrorMessage(err),
            sender: 'ai',
            time: new Date() });
          this.isLoading = false;
          this.cd.detectChanges();
        }
      });
  }

  /** Map the backend's RFC 7807 failure contract to a friendly chat bubble. */
  private buildErrorMessage(err: HttpErrorResponse): string {
    const problem = parseProblemDetails(err);

    let text: string;
    if (err.status === 502) {
      // LlmUnavailableException -> 502 (Day 20 backend work)
      text = 'The AI service is unavailable right now. Please try again in a moment.';
    } else if (err.status === 400 && problem?.errors) {
      // ValidationProblemDetails: show the field errors themselves.
      text = Object.values(problem.errors).flat().join(' ');
    } else if (err.status === 0) {
      text = 'I could not reach the server. Check your connection and try again.';
    } else {
      text = problem?.title ?? 'Sorry, I encountered an error.';
    }

    // Surface the correlation id so a failing request can be found in the logs.
    if (problem?.correlationId) {
      text += `\n\n\`ref: ${problem.correlationId}\``;
    }
    return text;
  }

  startNewChat(): void {
    this.currentThreadId = null;
    this.messages = [
      { text: 'Hello! How can I help you today?', sender: 'ai', time: new Date() }
    ];
    this.showThreadList = false;
  }

  deleteThread(threadId: string, event: Event): void {
    event.stopPropagation();
    this.threads = this.threads.filter(t => t.threadId !== threadId);
    if (this.currentThreadId === threadId) {
      this.startNewChat();
    }
    this.chatService.deleteThread(threadId).subscribe();
  }

  private loadMessages(threadId: string): void {
    this.chatService.getMessages(threadId).subscribe({
      next: (msgs) => {
        this.messages = msgs.length
          ? msgs.map(m => ({ text: m.text, sender: m.sender, time: new Date() }))
          : [{ text: 'Hello! How can I help you today?', sender: 'ai', time: new Date() }];
        this.cd.detectChanges();
      },
      error: () => {
        this.messages = [
          { text: 'Sorry, I could not load this conversation.', sender: 'ai', time: new Date() }
        ];
        this.cd.detectChanges();
      }
    });
  }

  formatMessage(text: string): string {
    return marked.parse(text) as string;
  }
}
