import { Component } from '@angular/core';
import { take } from 'rxjs';
import { Message } from '../_models/message';
import { Pagination } from '../_models/pagination';
import { AccountService } from '../_services/account.service';
import { MessageService } from './../_services/message.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css'],
})
export class MessagesComponent {
  messages: Message[] | undefined;
  pagination: Pagination | undefined;
  loading = false;
  container = 'Unread';
  pageNumber = 1;
  pageSize = 8;
  currentUsername: string | undefined;

  constructor(
    private messageService: MessageService,
    private accountService: AccountService
  ) {}

  ngOnInit(): void {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: (user) => (this.currentUsername = user?.knownAs.toLowerCase()),
    });
    this.loadMessages();
  }

  loadMessages() {
    this.loading = true;
    this.messageService
      .getMessages(this.pageNumber, this.pageSize, this.container)
      .subscribe({
        next: (response) => {
          this.messages = response.result;
          this.pagination = response.pagination;
          this.loading = false;
        },
      });
  }

  deleteMessage(id: number) {
    this.messageService.deleteMessage(id).subscribe({
      next: () =>
        this.messages?.splice(
          this.messages.findIndex((m) => m.id === id),
          1
        ),
    });
  }

  pageChanged(event: any) {
    if (this.pageNumber !== event.page) {
      this.pageNumber = event.page;
      this.loadMessages();
    }
  }
}
