import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { TimeagoModule } from 'ngx-timeago';
import { Message } from 'src/app/_models/message';

@Component({
  selector: 'app-member-messages',
  standalone: true,
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css'],
  imports: [CommonModule, TimeagoModule],
})
export class MemberMessagesComponent {
  @Input() messages: Message[] = [];
  @Input() username?: string;

  constructor() {}
}
