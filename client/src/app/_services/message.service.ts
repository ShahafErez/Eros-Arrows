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
}
