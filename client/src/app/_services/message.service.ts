import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Message } from '../_models/message';
import {
  getPaginatedResult,
  getPaginationHeaders,
} from '../_serviecs/PaginationHelper';

@Injectable({
  providedIn: 'root',
})
export class MessageService {
  baseUrlMessages = environment.apiUrl + '/messages';

  constructor(private http: HttpClient) {}

  getMessages(pageNumber: number, pageSize: number, container: string) {
    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('Container', container);
    return getPaginatedResult<Message[]>(
      this.baseUrlMessages,
      params,
      this.http
    );
  }

  getMessageThread(username: string) {
    return this.http.get<Message[]>(
      `${this.baseUrlMessages}/thread/${username}`
    );
  }

  sendMessage(username: string, content: string) {
    return this.http.post<Message>(this.baseUrlMessages, {
      recipientUsername: username,
      content,
    });
  }

  deleteMessage(id: number) {
    return this.http.delete(`${this.baseUrlMessages}/${id}`);
  }
}
