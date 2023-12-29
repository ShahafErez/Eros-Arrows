import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, of } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { environment } from 'src/environments/environment';
import { PaginatedResult } from '../_models/pagination';
import { UserParams } from './../_models/userParams';

@Injectable({
  providedIn: 'root',
})
export class MembersService {
  baseUrlUsers = environment.apiUrl + '/users';
  members: Member[] = [];

  constructor(private http: HttpClient) {}

  getMembers(userParams: UserParams) {
    let queryParams = this.getPaginationHeaders(
      userParams.pageNumber,
      userParams.pageSize
    );

    queryParams = queryParams.append('minAge', userParams.minAge);
    queryParams = queryParams.append('maxAge', userParams.maxAge);
    queryParams = queryParams.append('gender', userParams.gender);
    queryParams = queryParams.append('orderBy', userParams.orderBy);

    return this.getPaginatedResult<Member[]>(this.baseUrlUsers, queryParams);
  }

  private getPaginatedResult<T>(url: string, queryParams: HttpParams) {
    const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();
    return this.http
      .get<T>(url, {
        observe: 'response',
        params: queryParams,
      })
      .pipe(
        map((response) => {
          if (response.body) {
            paginatedResult.result = response.body;
          }
          const pagination = response.headers.get('Pagination');
          if (pagination) {
            paginatedResult.pagination = JSON.parse(pagination);
          }
          return paginatedResult;
        })
      );
  }

  private getPaginationHeaders(pageNumber: number, pageSize: number) {
    let queryParams = new HttpParams();
    queryParams = queryParams.append('pageNumber', pageNumber);
    queryParams = queryParams.append('pageSize', pageSize);
    return queryParams;
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
