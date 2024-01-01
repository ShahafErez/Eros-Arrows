import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { TimeagoModule } from 'ngx-timeago';
import { MessageService } from './../../_services/message.service';

@Component({
  selector: 'app-member-messages',
  standalone: true,
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css'],
  imports: [CommonModule, TimeagoModule, FormsModule],
})
export class MemberMessagesComponent {
  @Input('messageForm') messageForm?: NgForm;
  @Input() username?: string;
  messageContent = '';

  constructor(public messageService: MessageService) {}

  sendMessage() {
    if (!this.username) return;
    this.messageService
      .sendMessage(this.username, this.messageContent)
      .then(() => {
        this.messageForm?.reset();
        this.messageContent = '';
      });
  }
}
