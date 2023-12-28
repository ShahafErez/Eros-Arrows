import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, of } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { environment } from 'src/environments/environment';
import { PaginatedResult } from '../_models/pagination';

@Injectable({
  providedIn: 'root',
})
export class MembersService {
  baseUrlUsers = environment.apiUrl + '/users';
  members: Member[] = [];
  paginatedResult: PaginatedResult<Member[]> = new PaginatedResult<Member[]>();

  constructor(private http: HttpClient) {}

  getMembers(page?: number, itemsPerPage?: number) {
    let queryParams = new HttpParams();

    if (page && itemsPerPage) {
      queryParams = queryParams.append('pageNumber', page);
      queryParams = queryParams.append('pageSize', itemsPerPage);
    }

    return this.http
      .get<Member[]>(this.baseUrlUsers, {
        observe: 'response',
        params: queryParams,
      })
      .pipe(
        map((response) => {
          if (response.body) {
            this.paginatedResult.result = response.body;
          }
          const pagination = response.headers.get('Pagination');
          if (pagination) {
            this.paginatedResult.pagination = JSON.parse(pagination);
          }
          return this.paginatedResult;
        })
      );
  }

  getMember(username: string) {
    const member = this.members.find((x) => x.userName === username);
    if (member) return of(member);
    return this.http.get<Member>(`${this.baseUrlUsers}/${username}`);
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrlUsers, member).pipe(
      map(() => {
        const index = this.members.indexOf(member);
        this.members[index] = { ...this.members[index], ...member };
      })
    );
  }

  setMainPhoto(photoId: number) {
    return this.http.put(`${this.baseUrlUsers}/set-main-photo/${photoId}`, {});
  }

  deletePhoto(photoId: number) {
    return this.http.delete(`${this.baseUrlUsers}/delete-photo/${photoId}`);
  }
}
